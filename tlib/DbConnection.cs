using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using TLib.Exceptions;
using TLib.Dao;
using System.Data.SqlClient;
using System.Data;
using TLib.Logging;
using System.Data.Common;
using TLib.Container;

namespace TLib
{
    public interface IDbConnection2 : IDbConnection
    {
        DbConnection DbConnection
        {
            get;
            set;
        }
    }

    public abstract class AbstractDbConnection<T> : IDbConnection2, IPoolable
    {
        private string url = null;
        protected T conn;
        public T Connection
        {
            get { return conn; }
            set { conn = value; }
        }

        public DbConnection DbConnection
        {
            get { object dbc = conn; return (DbConnection)dbc; }
            set { object dbc = conn; dbc = value; }
        }

        public AbstractDbConnection(T c, string dbUrl)
        {
            try
            {
                conn = c;
                if (null != dbUrl) this.SetupObject(new string[] { dbUrl });
            }
            catch (Exception e)
            {
                DbConnection.Dispose();
                throw new DBConnectionException<T>(e, "{0} -- error conneting to {1}",
                    typeof(T).FullName,
                    dbUrl);
            }
        }

        public virtual void SetupObject(params object[] setupParameters)
        {
            if (null == this.url // not yet setup
                && null != setupParameters // valid setup params
                && setupParameters.Length > 0)
            {
                this.url = setupParameters[0].ToString();
                DbConnection.ConnectionString = this.url;
                DbConnection.Open();
                Logger.WriteLine("{0}::SetupObject({1}) called...", this.GetType().Name, setupParameters);
            }
        }

        public event EventHandler Disposing;
        private bool available = true;
        public bool Available
        {
            get { return available; }
            set { available = value; }
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Disposing != null)
                {
                    Disposing(this, new EventArgs());
                }
            }
        }

        public void Dispose()
        {
            //Dispose(true);
            DbConnection.Close();
            DbConnection.Dispose();
        }

        ~AbstractDbConnection()
        {
            Dispose(true);
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return DbConnection.BeginTransaction(il);
        }

        public IDbTransaction BeginTransaction()
        {
            return DbConnection.BeginTransaction();
        }

        public void ChangeDatabase(string databaseName)
        {
            DbConnection.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            DbConnection.Close();
        }

        public string ConnectionString
        {
            get
            {
                return DbConnection.ConnectionString;
            }
            set
            {
                DbConnection.ConnectionString = value;
            }
        }

        public int ConnectionTimeout
        {
            get { return DbConnection.ConnectionTimeout; }
        }

        public IDbCommand CreateCommand()
        {
            return DbConnection.CreateCommand();
        }

        public string Database
        {
            get { return DbConnection.Database; }
        }

        public void Open()
        {
            DbConnection.Open();
        }

        public ConnectionState State
        {
            get { return DbConnection.State; }
        }
    }

    public class SqlConnection2 : AbstractDbConnection<SqlConnection>
    {
        public SqlConnection2() : this(null) { }

        public SqlConnection2(string dbUrl)
            : base(new SqlConnection(), dbUrl)
        {
        }

        //private static SqlConnection2 instance = null;
        public static SqlConnection2 New()
        {
            return SqlConnectionPool.GetInstance().checkOut(null);
            //return new SqlConnection2();
            //if (null == instance) instance = new SqlConnection2();
            //instance.SetupObject(null);
            //return instance;
        }
    }

    public class OleConnection2 : AbstractDbConnection<OleDbConnection>
    {
        public static string URL(string location = DBConnection.OLEDB_CXN_STR)
        {
            return string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}", location);
        }

        protected OleConnection2(string url)
            : base(new OleDbConnection(), url)
        {
        }

        //private static OleConnection2 instance = null;
        public static OleConnection2 New()
        {
            return OleConnectionPool.GetInstance().checkOut(null);
            //if (null == instance) instance = new OleConnection2();
            //instance.SetupObject(null);
            //return instance;
        }
    }

    /// <summary>
    /// Summary description for DataAccessClass.
    /// </summary>
    public class DBConnection
    {
        /// <summary>
        /// OleDbConnection objConn = new OleDbConnection(sConnectionString); 
        /// objConn.Open(); 
        /// OleDbCommand objCmdSelect =new OleDbCommand("SELECT * FROM [Sheet1$]", objConn);
        /// OleDbDataAdapter objAdapter1 = new OleDbDataAdapter(); 
        /// objAdapter1.SelectCommand = objCmdSelect; 
        /// DataSet objDataset1 = new DataSet(); 
        /// objAdapter1.Fill(objDataset1); 
        /// objConn.Close(); 
        /// </summary>
        //String sConnectionString =  
        //    "Provider=Microsoft.Jet.OLEDB.4.0;" + 
        //    "Data Source=" + [Your Excel File Name Here] + ";" + 
        //    "Extended Properties=Excel 8.0;"; 

        public const string OLEDB_CXN_STR = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=..\\..\\database.mdb";
        //public const string SQLDB_CXN_STR = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=..\\..\\database.mdb";
        //public const string DB_CONN_STRING =
        //            "Driver={Microsoft Access Driver (*.mdb)}; " +
        //            "DBQ=..\\..\\database.mdb";

        protected string table;
        protected OleDbConnection con;

        public DBConnection(string t, OleDbConnection c = null)
        {
            this.table = t;
            //this.con = (null == c) ? newConnection(OLEDB_CXN_STR) : c;
        }

        ~DBConnection()  // destructor
        {
            // cleanup statements...
        }

        //public static OleDbConnection newConnection()
        //{
        //    return newConnection("..\\..\\database.mdb");
        //}


        public static OleDbConnection newConnection(string location = DBConnection.OLEDB_CXN_STR)
        {
            OleDbConnection con = null;
            try
            {
                string db = string.Format(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}", location);
                //con = new OleDbConnection(DBConnection.OLEDB_CXN_STR);
                con = new OleDbConnection(db);
            }
            catch (Exception ex)
            {
                throw new DBConnectionException<OleDbConnection>(ex, "Error : " + ex.Message);
                //connectection failed
            }//try-catch	  
            return con;
            //connection ok!
        }
    }
}
