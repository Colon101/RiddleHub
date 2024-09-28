
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;

/// <summary>
/// Summary description for Helper
/// </summary>
/// 


public class Helper
{
    public static SqlConnection ConnectToDb(string fileName)
    {
        string path = HttpContext.Current.Server.MapPath("App_Data/") + fileName;
        //string connString = @"Data Source=.\SQLEXPRESS;AttachDbFileName=" + path + ";Integrated Security=True;User Instance=True";
        //string connString = @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = |DataDirectory|\" + fileName + " Integrated Security = True";
        //string connString = @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = " + path + " Integrated Security = True";

        //string connString = @"";

        string connString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + path + ";Integrated Security=True;Connect Timeout=30";

        SqlConnection conn = new SqlConnection(connString);
        return conn;
    }

    public static string GenerateConnectionString(string fileName)
    {

        string path = HttpContext.Current.Server.MapPath("App_Data/") + fileName;

        return @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + path + ";Integrated Security=True;Connect Timeout=30";
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