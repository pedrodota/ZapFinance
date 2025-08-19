@echo off
echo ========================================
echo ZapFinance - Database Setup Script
echo ========================================
echo.

echo Checking if SQL Server is running...
sqlcmd -S localhost -E -Q "SELECT @@VERSION" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: SQL Server is not running or not accessible.
    echo Please make sure SQL Server is installed and running locally.
    echo.
    echo You can also try connecting with SQL Server Authentication:
    echo sqlcmd -S localhost -U sa -P YourPassword
    pause
    exit /b 1
)

echo SQL Server is running!
echo.

echo Creating ZapFinance database...
sqlcmd -S localhost -E -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ZapFinanceDb') CREATE DATABASE ZapFinanceDb;"

if %ERRORLEVEL% EQU 0 (
    echo ✓ Database 'ZapFinanceDb' created successfully!
) else (
    echo ✗ Failed to create database. Please check permissions.
    pause
    exit /b 1
)

echo.
echo Creating initial user and permissions...
sqlcmd -S localhost -E -d ZapFinanceDb -Q "IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'zapfinance_user') CREATE LOGIN zapfinance_user WITH PASSWORD = 'ZapFinance2024!', CHECK_POLICY = OFF;"
sqlcmd -S localhost -E -d ZapFinanceDb -Q "IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'zapfinance_user') CREATE USER zapfinance_user FOR LOGIN zapfinance_user;"
sqlcmd -S localhost -E -d ZapFinanceDb -Q "ALTER ROLE db_owner ADD MEMBER zapfinance_user;"

echo ✓ User 'zapfinance_user' created with full permissions!
echo.

echo ========================================
echo Database Setup Complete!
echo ========================================
echo.
echo Database Name: ZapFinanceDb
echo Server: localhost (or .\SQLEXPRESS if using SQL Express)
echo.
echo For DBeaver connection:
echo - Driver: SQL Server (Microsoft)
echo - Server: localhost
echo - Port: 1433
echo - Database: ZapFinanceDb
echo - Authentication: Windows Authentication (recommended)
echo   OR
echo - Username: zapfinance_user
echo - Password: ZapFinance2024!
echo.
echo Connection String for application:
echo Server=localhost;Database=ZapFinanceDb;Integrated Security=true;TrustServerCertificate=true;
echo.
echo OR with SQL Authentication:
echo Server=localhost;Database=ZapFinanceDb;User Id=zapfinance_user;Password=ZapFinance2024!;TrustServerCertificate=true;
echo.
pause
