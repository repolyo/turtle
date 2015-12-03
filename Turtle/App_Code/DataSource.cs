using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Data;

namespace Samples.AspNet.ObjectDataSource
{
    // 
    //  Northwind Employee Data Factory 
    // 
    public class NorthwindData : DbConn
    {
        const string sql = "SELECT ROW_NUMBER() OVER (ORDER BY a.CREATE_DATE DESC) AS ROWNO, " +
                "  a.tid, " +
                "  a.CREATE_DATE, " +
                "  ' ' as Filter, " +
                "  tc.TNAME, " +
                "  tc.TLOC " +
                "FROM " +
                " (SELECT " +
                "   t.tid, " +
                "   t.CREATE_DATE " +
                "  FROM " +
                "   TESTCASE_FUNC t, " +
                "   FUNC f " +
                "  WHERE " +
                "    t.fid = f.fid " +
                "    AND UPPER(f.FUNC_NAME) LIKE UPPER('{0}')" +
                "  UNION " +
                "   SELECT " +
                "    tt.tid, " +
                "    tt.CREATE_DATE " +
                "   FROM " +
                "    TAGS t, " +
                "    TESTCASE_TAGS tt " +
                "   WHERE " +
                "     t.tid = tt.tag_id " +
                "     AND UPPER(t.TAG_NAME) LIKE UPPER('{0}') " +
                "  ) a JOIN TESTCASE tc ON a.tid = tc.tid ";

        public NorthwindData()
        {
            NewConnection(Config.getConnectionString());
        }

        ~NorthwindData()
        {
            Terminate();
        }

        // Select all employees. 
        public DataTable QueryTestcases(string Filter, string sortColumns, int startRecord, int maxRecords)
        {
            DataTable table = Query(
                String.Format("SELECT * FROM ({0}) WHERE ROWNO > {1} AND ROWNO <= ({1} + {2})",
                    sql, startRecord, maxRecords),
                    (null == Filter) ? "%" : Filter);
            return table;
        }

        public int SelectCount(string Filter)
        {
            Object count = ExecuteScalar("SELECT count(*) FROM (" + sql + ")", 
                (null == Filter) ? "%" : Filter);
            return int.Parse(count.ToString());
        }
    }

    public class TestcaseData : DbConn
    {
        private const string query = "SELECT " +
            "   ROW_NUMBER() OVER (ORDER BY tf.SEQ ASC) AS ROWNO, " +
            "   t.tid, " +
            "   f.* " +
            " FROM " +
            "   TESTCASE t, " +
            "   TESTCASE_FUNC tf, " +
            "   FUNC f " +
            " WHERE " +
            "   t.tid = {0} " +
            "   AND t.tid = tf.tid " +
            "   AND tf.fid = f.fid ";

        public TestcaseData()
        {
             NewConnection( Config.getConnectionString() );
        }

        ~TestcaseData()
        {
            Terminate();
        }

        public int SelectCount(string TID)
        {
            Object count = ExecuteScalar("SELECT count(*) FROM (" + query + ")",
                (null == TID) ? "%" : TID);
            return int.Parse(count.ToString());
        }

        public DataTable QueryFunctions(string TID, string sortColumns, int startRecord, int maxRecords)
        {
            DataTable table = Query(
                String.Format("SELECT * FROM ({0}) WHERE ROWNO > {1} AND ROWNO <= ({1} + {2})",
                    query, startRecord, maxRecords),
                    (null == TID) ? "%" : TID);
            return table;
        }
    }
}