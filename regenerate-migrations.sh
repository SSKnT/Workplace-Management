#!/bin/bash

echo "Backing up and removing old migrations..."
rm -rf Migrations.backup
mv Migrations Migrations.backup

echo "Setting DATABASE_URL for PostgreSQL..."
export DATABASE_URL="postgresql://postgres.vmfhtbeaeijlsaslttmt:Admin123456@aws-0-us-east-1.pooler.supabase.com:6543/postgres"

echo "Creating fresh PostgreSQL migration..."
dotnet ef migrations add InitialCreatePostgres

echo "Done! New migration created for PostgreSQL."
echo "Review the migration, then commit and push to deploy."
