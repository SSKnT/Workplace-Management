# Attendance System Setup

## Prerequisites
- .NET 8.0 SDK
- PostgreSQL installed and running

## Database Setup

1. **Update Connection String**
   - Open `appsettings.json`
   - Update the connection string with your PostgreSQL credentials:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=AttendanceDB;Username=your_username;Password=your_password"
   }
   ```

2. **Create Database Migration**
   ```bash
   dotnet ef migrations add InitialCreate
   ```

3. **Update Database**
   ```bash
   dotnet ef database update
   ```

## Running the Application

```bash
dotnet run
```

## Default Admin Account

After running the application for the first time, you can login with:
- **Email**: admin@attendance.com
- **Password**: Admin123

## User Roles

The system supports three roles:
- **Admin**: Full system access
- **Teacher**: Can manage classes and attendance
- **Student**: Can view their attendance

## Next Steps

1. Make sure PostgreSQL is running
2. Update the connection string in `appsettings.json`
3. Run the migrations
4. Start the application
5. Register new users or login with the admin account

## Development Notes

- Password requirements are relaxed for development (minimum 4 characters)
- For production, update password policies in `Program.cs`
