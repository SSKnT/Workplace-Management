# Attendance System - Project Structure

## 📁 Directory Structure

```
AttendanceSystem/
├── Controllers/
│   ├── AccountController.cs          # Handles login, register, logout
│   └── HomeController.cs              # Default home controller
├── Data/
│   ├── ApplicationDbContext.cs        # EF Core DbContext with Identity
│   └── DbSeeder.cs                    # Seeds roles and admin user
├── Models/
│   ├── ApplicationUser.cs             # Custom user model (extends IdentityUser)
│   └── ErrorViewModel.cs              # Error handling model
├── ViewModels/
│   ├── LoginViewModel.cs              # Login form model
│   └── RegisterViewModel.cs           # Registration form model
├── Views/
│   ├── Account/
│   │   ├── Login.cshtml               # Login page
│   │   ├── Register.cshtml            # Registration page
│   │   └── AccessDenied.cshtml        # Access denied page
│   ├── Home/
│   │   ├── Index.cshtml               # Home page
│   │   └── Privacy.cshtml             # Privacy page
│   └── Shared/
│       ├── _Layout.cshtml             # Main layout with auth nav
│       └── Error.cshtml               # Error page
├── Migrations/                         # EF Core migrations
├── wwwroot/                           # Static files (CSS, JS, images)
├── appsettings.json                   # App configuration & connection string
├── Program.cs                         # App startup & configuration
└── README.md                          # Setup instructions
```

## 🔧 Technologies Used

- **ASP.NET Core 8.0 MVC** - Web framework
- **Entity Framework Core 8.0** - ORM
- **PostgreSQL** - Database (via Npgsql)
- **ASP.NET Core Identity** - Authentication & Authorization
- **Bootstrap 5** - UI framework

## 🔐 Authentication Features

### Implemented:
- User registration with email and password
- User login with "Remember Me" option
- User logout
- Password validation (relaxed for development)
- Role-based authorization (Admin, Teacher, Student)
- Cookie-based authentication
- Auto-seeding of roles and admin account

### Security Configuration:
```csharp
// Current password requirements (in Program.cs)
Password.RequiredLength = 4;          // Minimum 4 characters
Password.RequireDigit = false;         // No digit required
Password.RequireLowercase = false;     // No lowercase required
Password.RequireUppercase = false;     // No uppercase required
Password.RequireNonAlphanumeric = false; // No special character required
```

## 📊 Database Schema

### Tables Created by Identity:
- **AspNetUsers** - User accounts (includes FullName, CreatedAt)
- **AspNetRoles** - Roles (Admin, Teacher, Student)
- **AspNetUserRoles** - User-Role mapping
- **AspNetUserClaims** - User claims
- **AspNetUserLogins** - External login providers
- **AspNetUserTokens** - Authentication tokens
- **AspNetRoleClaims** - Role claims

## 🎯 Default Credentials

**Admin Account:**
- Email: admin@attendance.com
- Password: Admin123
- Role: Admin

## 🚀 Running the Application

1. **Update PostgreSQL connection** in `appsettings.json`
2. **Apply migrations**: `dotnet ef database update`
3. **Run**: `dotnet run`
4. **Access**: Navigate to the URL shown in terminal

## 📝 Key Files Explained

### Program.cs
- Configures services (MVC, EF Core, Identity)
- Sets up PostgreSQL connection
- Configures authentication cookies
- Seeds initial data (roles & admin)

### ApplicationDbContext.cs
- Inherits from IdentityDbContext
- Manages database context
- Will contain DbSets for your entities (Courses, Attendance, etc.)

### ApplicationUser.cs
- Extends IdentityUser
- Adds custom properties (FullName, CreatedAt)
- Can be extended with more properties as needed

### AccountController.cs
- Handles authentication flows
- Register: Creates new users with "Student" role
- Login: Authenticates users
- Logout: Signs users out

## 🎨 UI Components

- Responsive navigation with auth status
- Bootstrap-styled forms
- Validation messages
- User-friendly error pages

## 📈 What's Next?

Based on the CCP requirements, you should implement:
1. Course/Subject management
2. Enrollment system
3. Attendance marking
4. Reports and analytics
5. Teacher dashboard
6. Student dashboard
7. Admin panel

Ready to proceed step by step!
