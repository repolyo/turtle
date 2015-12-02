using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.Configuration;
using System.Data;
using System.Collections.Specialized;

namespace Samples.AspNet.ObjectDataSource
{
    // 
    //  Northwind Employee Data Factory 
    // 
    public class NorthwindData
    {
        const string sql = "SELECT ROW_NUMBER() OVER (ORDER BY a.tid) AS ROWNO, " +
                "  a.tid, " +
                "  ' ' as Filter, " +
                "  tc.TNAME, " +
                "  tc.TLOC " +
                "FROM " +
                " (SELECT " +
                "   t.tid " +
                "  FROM " +
                "   TESTCASE_FUNC t, " +
                "   FUNC f " +
                "  WHERE " +
                "    t.fid = f.fid " +
                "    AND UPPER(f.FUNC_NAME) LIKE UPPER('{0}') " +
                "  UNION " +
                "   SELECT " +
                "    tt.tid " +
                "   FROM " +
                "    TAGS t, " +
                "    TESTCASE_TAGS tt " +
                "   WHERE " +
                "     t.tid = tt.tag_id " +
                "     AND UPPER(t.TAG_NAME) LIKE UPPER('{0}') " +
                "  ) a JOIN TESTCASE tc ON a.tid = tc.tid ";

        private string _connectionString;

        public NorthwindData() {
            Initialize();
        }

        public void Initialize() {
            // Initialize data source. Use "Northwind" connection string from configuration.
             NameValueCollection settings = WebConfigurationManager.AppSettings;
             if (settings.Count <= 0) {
                 throw new Exception("A connection string named 'Oracle DB' with a valid connection string " +
                                     "must exist in the <appSettings> configuration section for the application.");
             }

            _connectionString = String.Format(
                "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT=1521)))" +
                "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={1})));" +
                "User Id={2};Password={3};", 
                    settings["db_host"],
                    settings["svc_name"],
                    settings["db_user"],
                    settings["db_pass"]);
        }


        // Select all employees. 
        public DataTable GetAllEmployees(string Filter, string sortColumns, int startRecord, int maxRecords)
        {
            DbConn.NewConnection(_connectionString);
            DataTable table = DbConn.Query(
                String.Format("SELECT * FROM ({0}) WHERE ROWNO > {1} AND ROWNO <= ({1} + {2})",
                    sql, startRecord, maxRecords),
                    (null == Filter) ? "%" : Filter);

            DbConn.Terminate();
            return table;
        }

        public int SelectCount(string Filter)
        {
            DbConn.NewConnection(_connectionString);
            Object count = DbConn.ExecuteScalar("SELECT count(*) FROM (" + sql + ")", 
                (null == Filter) ? "%" : Filter);
            DbConn.Terminate();
            return int.Parse(count.ToString());
        }
    }
}