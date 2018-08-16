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
public abstract class AbstractOracleDBTable<T> : AbstractTableDB<T> where T: AbstractDataRow
{
    private static string url = Config.getConnectionString();
    internal static OraclConnectionPool.OracleDBConn connection = null;
    protected List<string> sort_columns;
    private int total_count;

    protected AbstractOracleDBTable() : base() 
    {
        sort_columns = new List<string>();
    }

    public AbstractOracleDBTable(DataTable tbl, string dbUrl)
        : base(tbl)
    {
        url = dbUrl;
        sort_columns = new List<string>();
    }

    protected override string[] sort()
    {
        return sort_columns.ToArray();
    }

    protected virtual T GetFilter(string Filter)
    {
        return (T)this.NewRow();
    }

    protected override string buildSelectSQL(T filter, StringArraySQL cols = null, List<DataColumn> where = null, List<DataColumn> sorting = null)
    {
        string sql = string.Empty;
        Range<int> range = filter.Range;
        if (null != range)
        {
            sql = string.Format("SELECT * FROM ({0}) WHERE ROWNO > {1} AND ROWNO <= ({1} + {2})",
                            base.buildSelectSQL(filter, cols, where, sorting),
                            range.Min, range.Max);
        }
        else
        {
            sql = base.buildSelectSQL(filter, cols, where, sorting);
        }

        return sql;
    }

    public virtual DataTable QueryTestcases(string Filter, string sortColumns, int startRecord, int maxRecords)
    {
        T filter = GetFilter(string.IsNullOrEmpty(Filter) ? "0" : Filter);
        filter.Range = new Range<int>(startRecord, 0, maxRecords);

        sort_columns.Clear();
        sort_columns.Add(sortColumns);

        return findAll(filter);
    }

    public int SelectCount(string Filter)
    {
        T filter = GetFilter(Filter);
        total_count = (int)base.peekResultCount(filter);
        return total_count;
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

    public int TotalCount
    {
        get { return total_count; }
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
