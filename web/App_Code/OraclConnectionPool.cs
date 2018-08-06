using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tlib.Dao;
using Oracle.ManagedDataAccess.Client;
using TLib.Container;
using TLib;

/// <summary>
/// Summary description for OracleConnection
/// </summary>
/// 
public class OraclConnectionPool : AbstractPool<OraclConnectionPool.OracleDBConn>
{
    public OraclConnectionPool()
        : base(2)
    {
    }

    /// <summary>
    /// The static instance of the object pool. Thankfully the
    /// use of generics eliminates the need for a hash for each
    /// different pool type
    /// </summary>
    private static OraclConnectionPool instance = null;
    public static OraclConnectionPool GetInstance()
    {
        return (null == instance) ? instance = new OraclConnectionPool() : instance;
    }

    public class OracleDBConn : AbstractDbConnection<OracleConnection>
    {
        public OracleDBConn() : this(Config.getConnectionString()) { }

        public OracleDBConn(string dbUrl)
            : base(new OracleConnection(dbUrl), dbUrl)
        {
        }

        public static OracleDBConn New()
        {
            return OraclConnectionPool.GetInstance().checkOut(null);
        }
    }
}
