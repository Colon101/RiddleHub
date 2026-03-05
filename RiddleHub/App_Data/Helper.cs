
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;

/// <summary>
/// Summary description for Helper
/// </summary>
/// 


public class Helper
{
    private const string DefaultConnectionName = "WindowsDevLocalDb";
    private const string ConnectionNameAppSettingKey = "RiddleHubConnectionStringName";
    private const string ConnectionNameEnvironmentVariable = "RIDDLEHUB_CONNECTION_NAME";
    private static readonly object SchemaInitLock = new object();
    private static readonly HashSet<string> InitializedConnections = new HashSet<string>(StringComparer.Ordinal);

    public static SqlConnection ConnectToDb(string fileName)
    {
        string connString = BuildConnectionString(fileName);
        EnsureSchemaInitialized(connString);
        SqlConnection conn = new SqlConnection(connString);
        return conn;
    }

    public static string GenerateConnectionString(string fileName)
    {
        return BuildConnectionString(fileName);
    }

    private static string BuildConnectionString(string fileName)
    {
        string connectionName = ResolveConnectionName();
        ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionName];

        if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new ConfigurationErrorsException(
                "Missing or empty connection string '" + connectionName + "'. Add it under <connectionStrings> in Web.config.");
        }

        string connectionString = settings.ConnectionString;
        string appDataPath = HttpContext.Current.Server.MapPath("App_Data/");

        connectionString = connectionString.Replace("|DataDirectory|", appDataPath.TrimEnd('\\', '/'));

        if (connectionString.Contains("{AppDataPath}"))
        {
            connectionString = connectionString.Replace("{AppDataPath}", appDataPath.TrimEnd('\\', '/'));
        }

        if (connectionString.Contains("{DatabaseFileName}"))
        {
            connectionString = connectionString.Replace("{DatabaseFileName}", fileName);
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
        if (!string.IsNullOrWhiteSpace(configuredName))
        {
            return configuredName.Trim();
        }

        return DefaultConnectionName;
    }

    private static void EnsureSchemaInitialized(string connectionString)
    {
        if (InitializedConnections.Contains(connectionString))
        {
            return;
        }

        lock (SchemaInitLock)
        {
            if (InitializedConnections.Contains(connectionString))
            {
                return;
            }

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);

            if (string.IsNullOrWhiteSpace(builder.AttachDBFilename) && !string.IsNullOrWhiteSpace(builder.InitialCatalog))
            {
                EnsureDatabaseExists(builder);
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(GetSchemaInitSql(), conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            InitializedConnections.Add(connectionString);
        }
    }

    private static void EnsureDatabaseExists(SqlConnectionStringBuilder appDbBuilder)
    {
        string dbName = appDbBuilder.InitialCatalog;
        SqlConnectionStringBuilder masterBuilder = new SqlConnectionStringBuilder(appDbBuilder.ConnectionString);
        masterBuilder.InitialCatalog = "master";

        using (SqlConnection masterConn = new SqlConnection(masterBuilder.ConnectionString))
        {
            masterConn.Open();
            using (SqlCommand cmd = new SqlCommand(
                @"
IF DB_ID(@DbName) IS NULL
BEGIN
    DECLARE @sql NVARCHAR(300);
    SET @sql = N'CREATE DATABASE ' + QUOTENAME(@DbName);
    EXEC(@sql);
END;",
                masterConn))
            {
                cmd.Parameters.Add("@DbName", SqlDbType.NVarChar, 128).Value = dbName;
                cmd.ExecuteNonQuery();
            }
        }
    }

    private static string GetSchemaInitSql()
    {
        return @"
IF OBJECT_ID(N'dbo.[user]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[user] (
        [username] NVARCHAR (300) NOT NULL,
        [password] NVARCHAR (300) NOT NULL,
        [email]    NVARCHAR (300) NOT NULL,
        PRIMARY KEY CLUSTERED ([username] ASC)
    );
END;

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
END;

IF NOT EXISTS (SELECT 1 FROM dbo.[user] WHERE [username] = N'demo')
BEGIN
    INSERT INTO dbo.[user] ([username], [password], [email])
    VALUES (N'demo', N'demo', N'demo@local');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.[riddle])
BEGIN
    INSERT INTO dbo.[riddle] ([riddle_text], [riddle_hint], [answer], [username]) VALUES
    (N'What has keys but can''t open locks?', N'Music', N'A piano', N'demo'),
    (N'I speak without a mouth and hear without ears. What am I?', N'Sound', N'An echo', N'demo'),
    (N'What can travel around the world while staying in one corner?', N'Letters', N'A stamp', N'demo');
END;";
    }

    public static void DoQuery(string fileName, string sql)
    {
        SqlConnection conn = ConnectToDb(fileName);
        conn.Open();
        SqlCommand com = new SqlCommand(sql, conn);
        com.ExecuteNonQuery();
        conn.Close();
    }



    public static bool IsExist(string fileName, string sql)
    {

        SqlConnection conn = ConnectToDb(fileName);
        conn.Open();
        SqlCommand com = new SqlCommand(sql, conn);
        SqlDataReader data = com.ExecuteReader();

        bool found = Convert.ToBoolean(data.Read());
        conn.Close();
        return found;

    }

    public static DataTable ExecuteDataTable(string fileName, string sql)
    {
        SqlConnection conn = ConnectToDb(fileName);
        conn.Open();

        DataTable dt = new DataTable();

        SqlDataAdapter tableAdapter = new SqlDataAdapter(sql, conn);

        tableAdapter.Fill(dt);


        return dt;
    }

}
