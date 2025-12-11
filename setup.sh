#!/bin/bash
# Quick Setup Script for Attendance System

echo "🎓 Attendance System - Quick Setup"
echo "==================================="
echo ""

# Check if PostgreSQL is running
echo "📊 Checking PostgreSQL status..."
if systemctl is-active --quiet postgresql; then
    echo "✅ PostgreSQL is running"
else
    echo "⚠️  PostgreSQL is not running. Starting it..."
    sudo systemctl start postgresql
fi

echo ""
echo "📝 Next steps:"
echo "1. Update database connection in appsettings.json if needed"
echo "2. Run migrations: dotnet ef database update"
echo "3. Start the app: dotnet run"
echo ""
echo "Default admin credentials:"
echo "  Email: admin@attendance.com"
echo "  Password: Admin123"
echo ""
