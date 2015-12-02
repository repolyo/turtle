using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oracle.ManagedDataAccess.Client;
using System.Data;

/// <summary>
/// Summary description for DbConn
/// </summary>
public class DbConn
{
	//
	// TODO: Add constructor logic here
	//
    private static OracleConnection conn;
    private static OracleCommand cmd;
    private static OracleDataAdapter da;
    private static DataSet ds;
    private static string oradb = "Data Source=(DESCRIPTION="
            + "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=chritan-win7)(PORT=1521)))"
            + "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=XE)));"
            + "User Id=tc_profiler;Password=tc_profiler;";

    public DbConn()
    {
        //
        // TODO: Add constructor logic 
        //
    }

    public static string NewConnection()
    {
        return NewConnection(oradb);
    }

    public static string NewConnection(string constr)
    {
        try
        {
            conn = new OracleConnection(constr);
            conn.Open();
        }
        catch (OracleException e)
        {
            return e.Message;
        }

        return conn.State.ToString();
    }

    public static DataTable Query(String fmt, params Object [] args) {
        string sql = String.Format(fmt, args);
        Console.WriteLine(sql);
        cmd = new OracleCommand(sql, conn);
        cmd.CommandType = CommandType.Text;
        da = new OracleDataAdapter(cmd);
        ds = new DataSet();

        da.Fill(ds);
        return ds.Tables[0];
    }

    public static Object ExecuteScalar(String fmt, params Object [] args) {
        Object ret = null;
        string sql = String.Format(fmt, args);

        IDbCommand command = new OracleCommand(sql, conn);
        command.CommandType = CommandType.Text;
        /*
        OracleParameter parameterSomeValue = new OracleParameter("SomeValue", OracleDbType.Varchar2, 40);
        parameterSomeValue.Direction = ParameterDirection.Input;
        parameterSomeValue.Value = "TheValueToLookFor";
        command.Parameters.Add(parameterSomeValue);
        command.Connection = conn;
        */
        ret = command.ExecuteScalar();
        return ret;
    }

    public static void Terminate()
    {
        conn.Close();
    }

    static void Main(string[] args)
    {
        string strConn = DbConn.NewConnection();
        DataTable emp = DbConn.Query("SELECT * FROM TESTCASES");
        for (int i = 0; i < emp.Rows.Count; i++)
        {
            DataRow row = emp.Rows[i];
            //Print first name and last name
            Console.WriteLine(row["TNAME"] + "\t\t " + row["TLOC"]);
        }
        DbConn.Terminate();

        Console.Read();
    }
}