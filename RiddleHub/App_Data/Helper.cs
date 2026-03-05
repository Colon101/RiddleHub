
using System;
using System.Configuration;
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

    public static SqlConnection ConnectToDb(string fileName)
    {
        string connString = BuildConnectionString(fileName);
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
