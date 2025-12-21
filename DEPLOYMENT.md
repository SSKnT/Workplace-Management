# Railway + Supabase Deployment Guide

## Local Development Setup ✅
Your app is now configured to:
- Use **Supabase PostgreSQL** for both development and production
- Migrations have been created and applied successfully

## Important: Supabase Connection Details
The app is configured to use:
- **Host**: `aws-0-ap-south-1.pooler.supabase.com`
- **Port**: `6543` (Transaction Pooler)
- **Database**: `postgres`
- **Username**: `postgres.vmfhtbeaeijlsaslttmt`

> **Note**: Using Transaction Pooler (port 6543) because Session Pooler is recommended only for IPv4 networks.

## Deployment Steps

### 1. Supabase Database (Already Configured) ✅
Your Supabase database is already set up and migrations have been applied.
- Connection string is in `appsettings.json` and `appsettings.Production.json`

### 2. Deploy to Railway
1. Go to [railway.app](https://railway.app) and sign in with GitHub
2. Click **New Project** → **Deploy from GitHub repo**
3. Select your repository: `SSKnT/Workplace-Management`
4. Railway will auto-detect the Dockerfile and start building

### 3. Configure Environment Variables in Railway
In your Railway project dashboard:
1. Go to **Variables** tab
2. Add this variable (optional, as app uses appsettings.Production.json):
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ```
   
   **Or** if you prefer using DATABASE_URL environment variable:
   ```
   DATABASE_URL=Host=aws-0-ap-south-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.vmfhtbeaeijlsaslttmt;Password=sohaib123_1;SSL Mode=Require;Trust Server Certificate=true
   ASPNETCORE_ENVIRONMENT=Production
   ```

### 4. Redeploy
After adding variables, Railway will automatically redeploy. Your app will:
- ✅ Run migrations automatically
- ✅ Seed admin user and roles
- ✅ Be available at `https://[your-app].up.railway.app`

## Default Admin Credentials
After deployment, login with:
- **Email**: `admin@attendance.com`
- **Password**: `Admin@123`

⚠️ **Change these credentials immediately after first login!**

## Custom Domain (Optional)
In Railway → **Settings** → **Domains**:
- Add custom domain or use the provided Railway subdomain
- HTTPS is automatic

## Database Migrations
Migrations run automatically on startup. To create new migrations locally:
```bash
dotnet ef migrations add MigrationName
```

## Local Testing
To test locally with SQLite:
```bash
dotnet run
```

To test with PostgreSQL locally:
```bash
export DATABASE_URL="postgresql://..."
dotnet run
```

## Monitoring
Check Railway logs for:
- Migration status
- Seeding results
- Application errors

## Cost Estimate
- **Railway**: $5/month free credit (enough for small apps)
- **Supabase**: Free tier (500MB database, unlimited API requests)
- **Total**: FREE for small projects! 🎉
