using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

public static class Helper
{
    private const string DefaultConnectionName = "WindowsDevLocalDb";
    private const string ConnectionNameAppSettingKey = "RiddleHubConnectionStringName";
    private const string ConnectionNameEnvironmentVariable = "RIDDLEHUB_CONNECTION_NAME";
    private const string LinuxConnectionName = "LinuxSqlServerDev";
    private const string SqlPasswordEnvironmentVariable = "RIDDLEHUB_SQL_PASSWORD";
    private static readonly object SchemaInitLock = new object();
    private static readonly HashSet<string> InitializedConnections =
        new HashSet<string>(StringComparer.Ordinal);

    public static SqlConnection ConnectToDb()
    {
        string connectionString = BuildConnectionString();
        EnsureSchemaInitialized(connectionString);
        return new SqlConnection(connectionString);
    }

    private static string BuildConnectionString()
    {
        string connectionName = ResolveConnectionName();
        ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionName];
        if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new ConfigurationErrorsException(
                "Missing or empty connection string '" + connectionName +
                "'. Add it under <connectionStrings> in Web.config.");
        }

        string connectionString = settings.ConnectionString;
        if (string.Equals(connectionName, LinuxConnectionName, StringComparison.OrdinalIgnoreCase))
        {
            string sqlPassword = Environment.GetEnvironmentVariable(SqlPasswordEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(sqlPassword))
            {
                throw new ConfigurationErrorsException(
                    "RIDDLEHUB_SQL_PASSWORD must be set for the LinuxSqlServerDev connection.");
            }

            SqlConnectionStringBuilder linuxBuilder = new SqlConnectionStringBuilder(connectionString);
            linuxBuilder.Password = sqlPassword;
            connectionString = linuxBuilder.ConnectionString;
        }

        return connectionString;
    }

    private static string ResolveConnectionName()
    {
        string environmentName = Environment.GetEnvironmentVariable(ConnectionNameEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(environmentName))
        {
            return environmentName.Trim();
        }

        string configuredName = ConfigurationManager.AppSettings[ConnectionNameAppSettingKey];
        return !string.IsNullOrWhiteSpace(configuredName)
            ? configuredName.Trim()
            : DefaultConnectionName;
    }

    private static void EnsureSchemaInitialized(string connectionString)
    {
        lock (SchemaInitLock)
        {
            if (InitializedConnections.Contains(connectionString))
            {
                return;
            }

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            if (string.IsNullOrWhiteSpace(builder.AttachDBFilename) &&
                !string.IsNullOrWhiteSpace(builder.InitialCatalog))
            {
                EnsureDatabaseExists(builder);
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(GetSchemaInitSql(), connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            InitializedConnections.Add(connectionString);
        }
    }

    private static void EnsureDatabaseExists(SqlConnectionStringBuilder applicationBuilder)
    {
        string databaseName = applicationBuilder.InitialCatalog;
        SqlConnectionStringBuilder masterBuilder =
            new SqlConnectionStringBuilder(applicationBuilder.ConnectionString);
        masterBuilder.InitialCatalog = "master";

        using (SqlConnection masterConnection = new SqlConnection(masterBuilder.ConnectionString))
        {
            masterConnection.Open();
            using (SqlCommand command = new SqlCommand(
                @"
IF DB_ID(@DbName) IS NULL
BEGIN
    DECLARE @sql NVARCHAR(300);
    SET @sql = N'CREATE DATABASE ' + QUOTENAME(@DbName);
    EXEC(@sql);
END;",
                masterConnection))
            {
                command.Parameters.Add("@DbName", SqlDbType.NVarChar, 128).Value = databaseName;
                command.ExecuteNonQuery();
            }
        }
    }

    private static string GetSchemaInitSql()
    {
        return @"
IF OBJECT_ID(N'dbo.[user]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[user] (
        [username]                NVARCHAR (300) NOT NULL,
        [password]                NVARCHAR (300) NOT NULL,
        [email]                   NVARCHAR (300) NOT NULL,
        [password_reset_required] BIT            NOT NULL CONSTRAINT [DF_user_password_reset_required] DEFAULT (0),
        [session_version]         INT            NOT NULL CONSTRAINT [DF_user_session_version] DEFAULT (1),
        PRIMARY KEY CLUSTERED ([username] ASC)
    );
END;

IF COL_LENGTH(N'dbo.[user]', N'password_reset_required') IS NULL
    ALTER TABLE dbo.[user] ADD [password_reset_required] BIT NOT NULL CONSTRAINT [DF_user_password_reset_required] DEFAULT (1) WITH VALUES;

IF COL_LENGTH(N'dbo.[user]', N'session_version') IS NULL
    ALTER TABLE dbo.[user] ADD [session_version] INT NOT NULL CONSTRAINT [DF_user_session_version] DEFAULT (1) WITH VALUES;

UPDATE dbo.[user]
SET [password_reset_required] = 1
WHERE [password] NOT LIKE N'pbkdf2-sha256$%';

IF OBJECT_ID(N'dbo.[riddle]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[riddle] (
        [riddle_id]   INT            IDENTITY (1, 1) NOT NULL,
        [riddle_text] NVARCHAR (MAX) NOT NULL,
        [riddle_hint] NVARCHAR (100) NULL,
        [answer]      NVARCHAR (100) NOT NULL,
        [username]    NVARCHAR (300) NOT NULL,
        PRIMARY KEY CLUSTERED ([riddle_id] ASC),
        CONSTRAINT [FK_UserRiddle] FOREIGN KEY ([username]) REFERENCES [dbo].[user] ([username])
    );
END;";
    }
}
