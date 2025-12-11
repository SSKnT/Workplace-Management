using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AttendanceSystem.Models;

namespace AttendanceSystem.Services;

public class PdfExportService
{
    public byte[] GenerateAttendanceReport(Course course, DateTime date, List<AttendanceRecord> records)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Text($"Attendance Report - {course.Name}")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(10);

                        // Course Information
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"Course Code: {course.CourseCode}").SemiBold();
                                col.Item().Text($"Credit Hours: {course.CreditHours}");
                                col.Item().Text($"Teacher: {course.Teacher?.FullName ?? "Not Assigned"}");
                            });
                            
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"Date: {date:MMMM dd, yyyy}").SemiBold();
                                col.Item().Text($"Total Students: {records.Count}");
                                col.Item().Text($"Present: {records.Count(r => r.Status == AttendanceStatus.Present)}");
                            });
                        });

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // Attendance Table
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);  // #
                                columns.RelativeColumn(3);   // Student Name
                                columns.RelativeColumn(2);   // Email
                                columns.RelativeColumn(1);   // Status
                                columns.RelativeColumn(2);   // Remarks
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("#").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("Student Name").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("Email").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("Status").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("Remarks").SemiBold();
                            });

                            // Rows
                            int index = 1;
                            foreach (var record in records.OrderBy(r => r.Student.FullName))
                            {
                                var bgColor = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                                
                                table.Cell().Background(bgColor).Padding(5).Text(index.ToString());
                                table.Cell().Background(bgColor).Padding(5).Text(record.Student.FullName);
                                table.Cell().Background(bgColor).Padding(5).Text(record.Student.Email ?? "");
                                
                                var statusColor = record.Status switch
                                {
                                    AttendanceStatus.Present => Colors.Green.Medium,
                                    AttendanceStatus.Absent => Colors.Red.Medium,
                                    AttendanceStatus.Late => Colors.Orange.Medium,
                                    _ => Colors.Black
                                };
                                
                                table.Cell().Background(bgColor).Padding(5)
                                    .Text(record.Status.ToString()).FontColor(statusColor).SemiBold();
                                table.Cell().Background(bgColor).Padding(5).Text(record.Remarks ?? "");
                                
                                index++;
                            }
                        });

                        // Summary
                        column.Item().PaddingTop(20).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Summary").SemiBold().FontSize(14);
                                col.Item().Text($"Present: {records.Count(r => r.Status == AttendanceStatus.Present)}");
                                col.Item().Text($"Absent: {records.Count(r => r.Status == AttendanceStatus.Absent)}");
                                col.Item().Text($"Late: {records.Count(r => r.Status == AttendanceStatus.Late)}");
                                col.Item().Text($"Total: {records.Count}");
                            });
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generated on ");
                        x.Span(DateTime.Now.ToString("MMMM dd, yyyy HH:mm")).SemiBold();
                    });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateCourseAttendanceSummary(Course course, List<ApplicationUser> students, Dictionary<string, List<AttendanceRecord>> attendanceByStudent)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Text($"Course Attendance Summary - {course.Name}")
                    .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(10);

                        // Course Information
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Course Code: {course.CourseCode}").SemiBold();
                            row.RelativeItem().Text($"Teacher: {course.Teacher?.FullName ?? "Not Assigned"}");
                            row.RelativeItem().Text($"Total Students: {students.Count}");
                        });

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                        // Student Summary Table
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);  // #
                                columns.RelativeColumn(3);   // Student Name
                                columns.RelativeColumn(1);   // Total Sessions
                                columns.RelativeColumn(1);   // Present
                                columns.RelativeColumn(1);   // Absent
                                columns.RelativeColumn(1);   // Late
                                columns.RelativeColumn(1);   // Attendance %
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("#").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("Student Name").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("Total").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("Present").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("Absent").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("Late").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2)
                                    .Padding(5).Text("Attend %").SemiBold();
                            });

                            // Rows
                            int index = 1;
                            foreach (var student in students.OrderBy(s => s.FullName))
                            {
                                var records = attendanceByStudent.ContainsKey(student.Id) 
                                    ? attendanceByStudent[student.Id] 
                                    : new List<AttendanceRecord>();
                                
                                var totalSessions = records.Count;
                                var present = records.Count(r => r.Status == AttendanceStatus.Present || r.Status == AttendanceStatus.Late);
                                var absent = records.Count(r => r.Status == AttendanceStatus.Absent);
                                var late = records.Count(r => r.Status == AttendanceStatus.Late);
                                var percentage = totalSessions > 0 ? (present * 100.0 / totalSessions) : 0;
                                
                                var bgColor = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                                
                                table.Cell().Background(bgColor).Padding(5).Text(index.ToString());
                                table.Cell().Background(bgColor).Padding(5).Text(student.FullName);
                                table.Cell().Background(bgColor).Padding(5).Text(totalSessions.ToString());
                                table.Cell().Background(bgColor).Padding(5).Text((present - late).ToString());
                                table.Cell().Background(bgColor).Padding(5).Text(absent.ToString());
                                table.Cell().Background(bgColor).Padding(5).Text(late.ToString());
                                
                                var percentColor = percentage >= 75 ? Colors.Green.Medium :
                                                 percentage >= 60 ? Colors.Orange.Medium : Colors.Red.Medium;
                                
                                table.Cell().Background(bgColor).Padding(5)
                                    .Text($"{percentage:F1}%").FontColor(percentColor).SemiBold();
                                
                                index++;
                            }
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generated on ");
                        x.Span(DateTime.Now.ToString("MMMM dd, yyyy HH:mm")).SemiBold();
                    });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateStudentAttendanceReport(ApplicationUser student, List<Course> courses, Dictionary<int, List<AttendanceRecord>> attendanceByCourse)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Column(column =>
                    {
                        column.Item().Text($"Student Attendance Report")
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);
                        column.Item().Text($"{student.FullName}")
                            .FontSize(14).FontColor(Colors.Grey.Darken2);
                        column.Item().Text($"{student.Email}")
                            .FontSize(12).FontColor(Colors.Grey.Medium);
                    });

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(20);

                        foreach (var course in courses.OrderBy(c => c.CourseCode))
                        {
                            var records = attendanceByCourse.ContainsKey(course.Id) 
                                ? attendanceByCourse[course.Id] 
                                : new List<AttendanceRecord>();
                            
                            var totalSessions = records.Count;
                            var presentCount = records.Count(r => r.Status == AttendanceStatus.Present || r.Status == AttendanceStatus.Late);
                            var absentCount = records.Count(r => r.Status == AttendanceStatus.Absent);
                            var lateCount = records.Count(r => r.Status == AttendanceStatus.Late);
                            var percentage = totalSessions > 0 ? (presentCount * 100.0 / totalSessions) : 0;

                            column.Item().Column(courseColumn =>
                            {
                                // Course Header
                                courseColumn.Item().Background(Colors.Blue.Lighten4).Padding(10).Row(row =>
                                {
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text($"{course.CourseCode} - {course.Name}").SemiBold().FontSize(14);
                                        col.Item().Text($"Teacher: {course.Teacher?.FullName ?? "Not Assigned"}").FontSize(10);
                                    });
                                    
                                    var percentColor = percentage >= 75 ? Colors.Green.Medium :
                                                     percentage >= 60 ? Colors.Orange.Medium : Colors.Red.Medium;
                                    
                                    row.ConstantItem(120).AlignRight().Text($"{percentage:F1}%")
                                        .FontSize(18).SemiBold().FontColor(percentColor);
                                });

                                // Statistics
                                courseColumn.Item().PaddingTop(5).PaddingBottom(10).Row(row =>
                                {
                                    row.RelativeItem().Text($"Total Sessions: {totalSessions}").FontSize(10);
                                    row.RelativeItem().Text($"Present: {presentCount - lateCount}").FontSize(10).FontColor(Colors.Green.Medium);
                                    row.RelativeItem().Text($"Late: {lateCount}").FontSize(10).FontColor(Colors.Orange.Medium);
                                    row.RelativeItem().Text($"Absent: {absentCount}").FontSize(10).FontColor(Colors.Red.Medium);
                                });

                                if (records.Any())
                                {
                                    // Attendance Details Table
                                    courseColumn.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.ConstantColumn(40);  // #
                                            columns.RelativeColumn(2);   // Date
                                            columns.RelativeColumn(1);   // Status
                                            columns.RelativeColumn(3);   // Remarks
                                        });

                                        // Header
                                        table.Header(header =>
                                        {
                                            header.Cell().Background(Colors.Grey.Lighten2)
                                                .Padding(5).Text("#").SemiBold().FontSize(10);
                                            header.Cell().Background(Colors.Grey.Lighten2)
                                                .Padding(5).Text("Date").SemiBold().FontSize(10);
                                            header.Cell().Background(Colors.Grey.Lighten2)
                                                .Padding(5).Text("Status").SemiBold().FontSize(10);
                                            header.Cell().Background(Colors.Grey.Lighten2)
                                                .Padding(5).Text("Remarks").SemiBold().FontSize(10);
                                        });

                                        // Rows
                                        int index = 1;
                                        foreach (var record in records.OrderByDescending(r => r.Date))
                                        {
                                            var bgColor = index % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;
                                            
                                            table.Cell().Background(bgColor).Padding(5).Text(index.ToString()).FontSize(9);
                                            table.Cell().Background(bgColor).Padding(5).Text(record.Date.ToString("MMM dd, yyyy")).FontSize(9);
                                            
                                            var statusColor = record.Status switch
                                            {
                                                AttendanceStatus.Present => Colors.Green.Medium,
                                                AttendanceStatus.Absent => Colors.Red.Medium,
                                                AttendanceStatus.Late => Colors.Orange.Medium,
                                                _ => Colors.Black
                                            };
                                            
                                            table.Cell().Background(bgColor).Padding(5)
                                                .Text(record.Status.ToString()).FontColor(statusColor).SemiBold().FontSize(9);
                                            table.Cell().Background(bgColor).Padding(5).Text(record.Remarks ?? "").FontSize(9);
                                            
                                            index++;
                                        }
                                    });
                                }
                                else
                                {
                                    courseColumn.Item().PaddingVertical(10).Text("No attendance records yet")
                                        .FontSize(10).Italic().FontColor(Colors.Grey.Medium);
                                }
                            });
                        }

                        // Overall Summary
                        if (courses.Any())
                        {
                            var allRecords = attendanceByCourse.Values.SelectMany(r => r).ToList();
                            var totalOverall = allRecords.Count;
                            var presentOverall = allRecords.Count(r => r.Status == AttendanceStatus.Present || r.Status == AttendanceStatus.Late);
                            var absentOverall = allRecords.Count(r => r.Status == AttendanceStatus.Absent);
                            var lateOverall = allRecords.Count(r => r.Status == AttendanceStatus.Late);
                            var percentageOverall = totalOverall > 0 ? (presentOverall * 100.0 / totalOverall) : 0;

                            column.Item().LineHorizontal(2).LineColor(Colors.Grey.Medium);
                            
                            column.Item().Background(Colors.Blue.Lighten5).Padding(15).Column(summaryCol =>
                            {
                                summaryCol.Item().Text("Overall Summary").SemiBold().FontSize(16);
                                summaryCol.Item().PaddingTop(5).Row(row =>
                                {
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text($"Total Courses: {courses.Count}");
                                        col.Item().Text($"Total Sessions: {totalOverall}");
                                    });
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text($"Present: {presentOverall - lateOverall}").FontColor(Colors.Green.Medium);
                                        col.Item().Text($"Late: {lateOverall}").FontColor(Colors.Orange.Medium);
                                    });
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text($"Absent: {absentOverall}").FontColor(Colors.Red.Medium);
                                        var percentColor = percentageOverall >= 75 ? Colors.Green.Medium :
                                                         percentageOverall >= 60 ? Colors.Orange.Medium : Colors.Red.Medium;
                                        col.Item().Text($"Attendance: {percentageOverall:F1}%").SemiBold().FontColor(percentColor);
                                    });
                                });
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Generated on ");
                        x.Span(DateTime.Now.ToString("MMMM dd, yyyy HH:mm")).SemiBold();
                    });
            });
        });

        return document.GeneratePdf();
    }
}
