using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLib.Container;
using TLib.Dao;
using Tlib.Dao;

namespace TLib.Dao
{
    public class SqlConnectionPool : AbstractPool<SqlConnection2>
    {
        public SqlConnectionPool()
            : base(2)
        {
        }

        /// <summary>
        /// The static instance of the object pool. Thankfully the
        /// use of generics eliminates the need for a hash for each
        /// different pool type
        /// </summary>
        private static SqlConnectionPool instance = null;
        public static SqlConnectionPool GetInstance()
        {
            return (null == instance) ? instance = new SqlConnectionPool() : instance;
        }
    }

    public class OleConnectionPool : AbstractPool<OleConnection2>
    {
        private OleConnectionPool()
            : base(2)
        {
        }

        /// <summary>
        /// The static instance of the object pool. Thankfully the
        /// use of generics eliminates the need for a hash for each
        /// different pool type
        /// </summary>
        private static OleConnectionPool instance = null;
        public static OleConnectionPool GetInstance()
        {
            return (null == instance) ? instance = new OleConnectionPool() : instance;
        }
    }
}
