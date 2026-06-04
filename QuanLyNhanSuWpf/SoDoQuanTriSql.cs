using Microsoft.Data.SqlClient;

namespace QuanLyNhanSuWpf;

public static class SoDoQuanTriSql
{
    public static async Task DamBaoAsync(SqlConnection ketNoi)
    {
        await using var lenh = new SqlCommand("""
            IF OBJECT_ID(N'dbo.HR_Users', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.HR_Users
                (
                    UserID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    Username NVARCHAR(80) NOT NULL UNIQUE,
                    FullName NVARCHAR(150) NOT NULL,
                    RoleName NVARCHAR(80) NOT NULL,
                    PasswordHash NVARCHAR(200) NOT NULL,
                    PasswordSalt NVARCHAR(200) NOT NULL,
                    PasswordIterations INT NOT NULL,
                    IsActive BIT NOT NULL DEFAULT(1),
                    RequirePasswordChange BIT NOT NULL DEFAULT(0),
                    FailedLoginCount INT NOT NULL DEFAULT(0),
                    LockoutUntilAt DATETIME2 NULL,
                    CreatedAt DATETIME2 NOT NULL DEFAULT(SYSUTCDATETIME()),
                    LastLoginAt DATETIME2 NULL
                );
            END;

            IF OBJECT_ID(N'dbo.HR_AuditLogs', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.HR_AuditLogs
                (
                    AuditID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    ActorUsername NVARCHAR(80) NULL,
                    ActionName NVARCHAR(120) NOT NULL,
                    EntityName NVARCHAR(120) NULL,
                    EntityKey NVARCHAR(120) NULL,
                    Detail NVARCHAR(1000) NULL,
                    MachineName NVARCHAR(120) NULL,
                    CreatedAt DATETIME2 NOT NULL DEFAULT(SYSUTCDATETIME())
                );
            END;

            IF COL_LENGTH(N'dbo.HR_Users', N'RequirePasswordChange') IS NULL
                ALTER TABLE dbo.HR_Users ADD RequirePasswordChange BIT NOT NULL DEFAULT(0);
            IF COL_LENGTH(N'dbo.HR_Users', N'FailedLoginCount') IS NULL
                ALTER TABLE dbo.HR_Users ADD FailedLoginCount INT NOT NULL DEFAULT(0);
            IF COL_LENGTH(N'dbo.HR_Users', N'LockoutUntilAt') IS NULL
                ALTER TABLE dbo.HR_Users ADD LockoutUntilAt DATETIME2 NULL;
            IF COL_LENGTH(N'dbo.HR_Users', N'LastLoginAt') IS NULL
                ALTER TABLE dbo.HR_Users ADD LastLoginAt DATETIME2 NULL;
            IF COL_LENGTH(N'dbo.HR_AuditLogs', N'MachineName') IS NULL
                ALTER TABLE dbo.HR_AuditLogs ADD MachineName NVARCHAR(120) NULL;
            """, ketNoi);

        await lenh.ExecuteNonQueryAsync();
    }
}
