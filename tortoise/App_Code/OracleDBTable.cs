using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Tlib.Dao;
using TLib.Interfaces;
using Oracle.ManagedDataAccess.Client;
using TLib;
using System.Data.Common;

/// <summary>
/// Summary description for AbstractOracleDBTable
/// </summary>
public abstract class AbstractOracleDBTable<T> : AbstractTableDB<T>
{
    private static string url = Config.getConnectionString();
    internal static OraclConnectionPool.OracleDBConn connection = null;

    protected AbstractOracleDBTable() : base() 
    {
    }

    public AbstractOracleDBTable(DataTable tbl, string dbUrl)
        : base(tbl)
    {
        url = dbUrl;
    }

    protected override IDbCommand2 newCommand(string sql)
    {
        if (null == connection)
        {
            connection = OraclConnectionPool.GetInstance().checkOut(new string[] { Config.getConnectionString() });
        }
        return new OracleDbCommand(new OracleCommand(sql, connection.Connection));
    }

    public override DataTable Clone()
    {
        AbstractTableDB<T> cln = ((AbstractTableDB<T>)(base.Clone()));
        cln.initialize();
        
        return cln;
    }

    public long peekResultCount()
    {
        object obj = this.NewRow();
        T filter = (T)obj;
        return base.peekResultCount(filter);
    }

    class OracleDbCommand : AbstractDbCommandBase, IDbCommand2
    {
        public OracleDbCommand(OracleCommand cmd) : base(cmd) { }
        ~OracleDbCommand()
        {
            OraclConnectionPool.GetInstance().checkIn(connection);
            System.Console.WriteLine("In OracleDbCommand's destructor.");
        }

        public System.Data.Common.DbParameter AddWithValue(string parameterName, object value)
        {
            throw new NotImplementedException();
        }

        public object find(System.Data.DataTable tbl)
        {
            throw new NotImplementedException();
        }

        public DataTable findAll(System.Data.DataTable tbl)
        {
            tbl.Load(base.ExecuteReader());
            return tbl;
        }

        public System.Data.IDbConnection Connection
        {
            get
            {
                return cmd.Connection;
            }
            set
            {
                cmd.Connection = (OracleConnection)value;
            }
        }

        public System.Data.IDbTransaction Transaction
        {
            get
            {
                return cmd.Transaction;
            }
            set
            {
                cmd.Transaction = (DbTransaction)value;
            }
        }

        public override void Dispose()
        {
           base.Dispose();
        }
    }
}
