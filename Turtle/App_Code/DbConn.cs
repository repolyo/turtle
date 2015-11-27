﻿using System;
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

    public DbConn()
    {
        //
        // TODO: Add constructor logic 
        //
    }

    public static string NewConnection()
    {
        try
        {
            //string oradb = " Data Source = XE; User Id = hr; Password = hr; ";
            string oradb = "Data Source=(DESCRIPTION="
+ "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=chritan-win7)(PORT=1521)))"
+ "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=XE)));"
+ "User Id=tc_profiler;Password=tc_profiler;";
            conn = new OracleConnection(oradb);
            conn.Open();
        }
        catch (OracleException e)
        {
            return e.Message;
        }

        return conn.State.ToString();
    }

    public static DataTable GetEmployees()
    {
        string SQL = " SELECT TNAME, TLOC FROM TESTCASE WHERE ROWNUM < 50";
        cmd = new OracleCommand(SQL, conn);
        cmd.CommandType = CommandType.Text;
        da = new OracleDataAdapter(cmd);
        ds = new DataSet();

        da.Fill(ds);
        return ds.Tables[0];
    }

    public static DataTable Query(String fmt, params Object [] args) {
        cmd = new OracleCommand(String.Format(fmt, args), conn);
        cmd.CommandType = CommandType.Text;
        da = new OracleDataAdapter(cmd);
        ds = new DataSet();

        da.Fill(ds);
        return ds.Tables[0];
    }

    public static void Terminate()
    {
        conn.Close();
    }

    static void Main(string[] args)
    {
        string strConn = DbConn.NewConnection();
        DataTable emp = DbConn.GetEmployees();
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