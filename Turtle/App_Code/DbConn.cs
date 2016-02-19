using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;

public class DBException : System.Exception
{
    public DBException(Exception innerException, string message, params Object[] args)
        : base(String.Format(message, args), innerException)
   {
   }
}
    

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

    private int _startIndex = 0;
    public int StartIndex
    {
        get
        {
            return _startIndex;
        }
        set
        {
            _startIndex = value;
        }
    }

    private int _endIndex = 0;
    public int EndIndex
    {
        get
        {
            return _endIndex;
        }
        set
        {
            _endIndex = value;
        }
    }

    private int _total = 0;
    public int TotalCount
    {
        get
        {
            return _total;
        }
        set
        {
            _total = value;
        }
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
        DataTable tbl = null;
        string sql = String.Format(fmt, args);
        try
        {
            Console.WriteLine(sql);
            cmd = new OracleCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            da = new OracleDataAdapter(cmd);
            ds = new DataSet();
            if (da.Fill(ds) > 0) {
                tbl = ds.Tables[0];
            }
        }
        catch (Exception e) {
            throw new DBException(e, sql, args);
        }
        return tbl;
    }

    public static int Update(String fmt, params Object[] args)
    {
        int updated = 0;
        string sql = String.Format(fmt, args);
        try
        {
            Console.WriteLine(sql);
            cmd = new OracleCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            /*cmd.CommandText = "update table set col1 = :param1, col2 = :param2 where key = :keyValue";
            cmd.Parameters.AddWithValue("param1", 1);
            cmd.Parameters.AddWithValue("param2", "Text data");
            cmd.Parameters.AddWithValue("keyValue", "1");
            */
            updated = cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            throw new DBException(e, sql, args);
        }
        return updated;
    }

    public static int Delete(String fmt, params Object[] args)
    {
        int deleted = 0;
        string sql = String.Format(fmt, args);
        try
        {
            Console.WriteLine(sql);
            cmd = new OracleCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            deleted = cmd.ExecuteNonQuery();
        }
        catch (Exception e)
        {
            throw new DBException(e, sql, args);
        }
        return deleted;
    }

    public static Object ExecuteScalar(String fmt, params Object [] args) {
        Object ret = null;
        string sql = String.Format(fmt, args);
        try {
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
        }
        catch (Exception e) {
             throw new DBException(e, sql, args);
        }
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