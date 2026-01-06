// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

//AJAX
function markAttendanceAjax(studentId, courseId, date, statusElement, remarksElement) {
    const status = parseInt(statusElement.value);
    const remarks = remarksElement.value;

    const data = {
        studentId: studentId,
        courseId: courseId,
        date: date,
        status: status,
        remarks: remarks
    };

    const row = statusElement.closest('tr');
    row.style.opacity = '0.6';

    fetch('/Attendance/MarkAttendanceAjax', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data)
    })
    .then(response => response.json())
    .then(result => {
        row.style.opacity = '1';
        
        if (result.success) {
            showNotification('success', result.message);
            
            row.classList.add('table-success');
            setTimeout(() => {
                row.classList.remove('table-success');
            }, 2000);
        } else {
            showNotification('error', result.message);
        }
    })
    .catch(error => {
        row.style.opacity = '1';
        showNotification('error', 'Failed to update attendance. Please try again.');
        console.error('Error:', error);
    });
}

function showNotification(type, message) {
    const existingNotifications = document.querySelectorAll('.ajax-notification');
    existingNotifications.forEach(n => n.remove());

    const notification = document.createElement('div');
    notification.className = `alert alert-${type === 'success' ? 'success' : 'danger'} alert-dismissible fade show ajax-notification`;
    notification.style.position = 'fixed';
    notification.style.top = '20px';
    notification.style.right = '20px';
    notification.style.zIndex = '9999';
    notification.style.minWidth = '300px';
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.remove();
    }, 3000);
}

document.addEventListener('DOMContentLoaded', function() {
    const attendanceForm = document.getElementById('attendanceForm');
    
    if (attendanceForm) {
        const statusSelects = attendanceForm.querySelectorAll('select[name*="Status"]');
        const remarksInputs = attendanceForm.querySelectorAll('input[name*="Remarks"]');
        
        // Add change event listeners to status dropdowns
        statusSelects.forEach((select, index) => {
            select.addEventListener('change', function() {
                const studentId = this.getAttribute('data-student-id');
                const courseId = this.getAttribute('data-course-id');
                const date = this.getAttribute('data-date');
                const remarksInput = remarksInputs[index];
                
                if (studentId && courseId && date) {
                    markAttendanceAjax(studentId, courseId, date, this, remarksInput);
                }
            });
        });
        
        remarksInputs.forEach((input, index) => {
            input.addEventListener('blur', function() {
                const studentId = this.getAttribute('data-student-id');
                const courseId = this.getAttribute('data-course-id');
                const date = this.getAttribute('data-date');
                const statusSelect = statusSelects[index];
                
                if (studentId && courseId && date) {
                    markAttendanceAjax(studentId, courseId, date, statusSelect, this);
                }
            });
        });
    }
});

