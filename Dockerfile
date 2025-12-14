# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["AttendanceSystem.csproj", "./"]
RUN dotnet restore "AttendanceSystem.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "AttendanceSystem.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AttendanceSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install necessary dependencies for PDF generation
RUN apt-get update && apt-get install -y \
    libgdiplus \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "AttendanceSystem.dll"]
