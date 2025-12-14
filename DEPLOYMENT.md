# Railway + Supabase Deployment Guide

## Local Development Setup ✅
Your app is now configured to:
- Use **SQLite** locally (development)
- Use **PostgreSQL** in production (Railway + Supabase)

## Deployment Steps

### 1. Create Supabase Database
1. Go to [supabase.com](https://supabase.com) and create a free account
2. Create a new project
3. Go to **Project Settings** → **Database**
4. Copy the **Connection String** (URI format):
   ```
   postgresql://postgres:[YOUR-PASSWORD]@db.[PROJECT-REF].supabase.co:5432/postgres
   ```

### 2. Deploy to Railway
1. Go to [railway.app](https://railway.app) and sign in with GitHub
2. Click **New Project** → **Deploy from GitHub repo**
3. Select your repository: `SSKnT/Workplace-Management`
4. Railway will auto-detect the Dockerfile and start building

### 3. Configure Environment Variables in Railway
In your Railway project dashboard:
1. Go to **Variables** tab
2. Add these variables:
   ```
   DATABASE_URL=postgresql://postgres:[PASSWORD]@db.[PROJECT-REF].supabase.co:5432/postgres
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
