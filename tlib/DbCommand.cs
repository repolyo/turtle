using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using TLib.Interfaces;
using Tlib.Dao;
using System.Data.OleDb;
using System.Data;
using System.Data.SqlClient;

namespace TLib
{
    public abstract class AbstractDbCommandBase
    {
        protected DbCommand cmd = null;

        protected AbstractDbCommandBase(DbCommand c)
        {
            cmd = c;
        }

        public void Cancel()
        {
            cmd.Cancel();
        }

        public string CommandText
        {
            get
            {
                return cmd.CommandText;
            }
            set
            {
                cmd.CommandText = value;
            }
        }

        public int CommandTimeout
        {
            get
            {
                return cmd.CommandTimeout;
            }
            set
            {
                cmd.CommandTimeout = value;
            }
        }

        public System.Data.CommandType CommandType
        {
            get
            {
                return cmd.CommandType;
            }
            set
            {
                cmd.CommandType = value;
            }
        }

        public System.Data.IDbDataParameter CreateParameter()
        {
            return cmd.CreateParameter();
        }

        public int ExecuteNonQuery()
        {
            return cmd.ExecuteNonQuery();
        }

        public System.Data.IDataReader ExecuteReader(System.Data.CommandBehavior behavior)
        {
            return cmd.ExecuteReader(behavior);
        }

        public System.Data.IDataReader ExecuteReader()
        {
            return cmd.ExecuteReader();
        }

        public object ExecuteScalar()
        {
            return cmd.ExecuteScalar();
        }

        public System.Data.IDataParameterCollection Parameters
        {
            get { return cmd.Parameters; }
        }

        public void Prepare()
        {
            cmd.Prepare();
        }

        public System.Data.UpdateRowSource UpdatedRowSource
        {
            get
            {
                return cmd.UpdatedRowSource;
            }
            set
            {
                cmd.UpdatedRowSource = value;
            }
        }

        public virtual void Dispose()
        {
            //cmd.Connection.Dispose();
            cmd.Dispose();
        }
    }

    public class SqlDbCommand : AbstractDbCommandBase, IDbCommand2
    {
        private SqlConnection2 myConn = null;
        public SqlDbCommand(string sql, SqlConnection2 conn)
            : base(new SqlCommand(sql, conn.Connection))
        {
            myConn = conn;
        }

        public System.Data.Common.DbParameter AddWithValue(string parameterName, object value)
        {
            SqlCommand sqlCmd = (SqlCommand)cmd;
            return sqlCmd.Parameters.AddWithValue(parameterName, value);
        }

        public object find(System.Data.DataTable tbl)
        {
            throw new NotImplementedException();
        }

        public DataTable findAll(System.Data.DataTable tbl)
        {
            throw new NotImplementedException();
        }

        public System.Data.IDbConnection Connection
        {
            get
            {
                return cmd.Connection;
            }
            set
            {
                cmd.Connection = (SqlConnection)value;
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
                cmd.Transaction = (SqlTransaction)value;
            }
        }

        public override void Dispose()
        {
            this.myConn.Dispose(true);
            base.Dispose();
        }
    }

    public class OLEDbCommand : AbstractDbCommandBase, IDbCommand2
    {
        private OleConnection2 myConn = null;
        public OLEDbCommand(string sql, OleConnection2 conn)
            : base(new OleDbCommand(sql, (OleDbConnection)conn.DbConnection))
        {
            this.myConn = conn;
        }

        public System.Data.Common.DbParameter AddWithValue(string parameterName, object value)
        {
            OleDbCommand oleCmd = (OleDbCommand)cmd;
            return oleCmd.Parameters.AddWithValue(parameterName, value);
        }

        public object find(System.Data.DataTable tbl)
        {
            throw new NotImplementedException();
        }

        public System.Data.DataTable findAll(System.Data.DataTable tbl)
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
                cmd.Connection = (OleDbConnection)value;
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
                cmd.Transaction = (OleDbTransaction)value;
            }
        }

        public override void Dispose()
        {
            this.myConn.Dispose(true);
            base.Dispose();
        }
    }

}
