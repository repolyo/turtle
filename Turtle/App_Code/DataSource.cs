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
    public class TestcaseProfileData : DbConn
    {
        const string queryFunc = "SELECT " +
                "   t.tid " +
                "  FROM " +
                "   TESTCASE_FUNC t, " +
                "   FUNC f " +
                "  WHERE " +
                "    t.fid = f.fid " +
                "    AND UPPER(f.FUNC_NAME) LIKE UPPER('{0}')";

        const string queryTags = "SELECT " +
                "    tt.tid " +
                "   FROM " +
                "    TAGS t, " +
                "    TESTCASE_TAGS tt " +
                "   WHERE " +
                "     t.tid = tt.tag_id " +
                "     AND UPPER(t.TAG_NAME) LIKE UPPER('{0}')";

        const string queryAll = "SELECT ROW_NUMBER() OVER (ORDER BY a.CREATE_DATE DESC) AS ROWNO, " +
                "  a.tid, " +
                "  a.CREATE_DATE, " +
                "  ' ' as Filter, " +
                "  a.TNAME, " +
                "  a.TLOC " +
                "FROM TESTCASE a WHERE a.tid IN (" + queryFunc + " UNION " + queryTags + " )";

        public TestcaseProfileData()
        {
            NewConnection(Config.getConnectionString());
        }

        ~TestcaseProfileData()
        {
            Terminate();
        }

        private string type = "";
        public string Type {
            set { type = value; }
            get{ return "0"; }
        }

        // Select all employees. 
        public DataTable QueryTestcases(string Filter, string sortColumns, int startRecord, int maxRecords)
        {
            string keyword = (null == Filter) ? "%" : Filter;
            string sql = null;
            switch (Config.filterType)
            {
                case FilterType.FUNC:
                    sql = "SELECT ROW_NUMBER() OVER (ORDER BY a.CREATE_DATE DESC) AS ROWNO, " +
                        "  a.tid, " +
                        "  a.CREATE_DATE, " +
                        "  ' ' as Filter, " +
                        "  a.TNAME, " +
                        "  a.TLOC " +
                        "FROM TESTCASE a WHERE a.tid IN (" + queryFunc + ")";
                    break;
                case FilterType.TAG:
                    sql = "SELECT ROW_NUMBER() OVER (ORDER BY a.CREATE_DATE DESC) AS ROWNO, " +
                        "  a.tid, " +
                        "  a.CREATE_DATE, " +
                        "  ' ' as Filter, " +
                        "  a.TNAME, " +
                        "  a.TLOC " +
                        "FROM TESTCASE a WHERE a.tid IN (" + queryTags + ")";
                    break;
                case FilterType.ALL:
                default:
                    sql = queryAll;
                    break;
            }
            return Query(String.Format("SELECT * FROM ({0}) WHERE ROWNO > {1} AND ROWNO <= ({1} + {2})",
                                sql, startRecord, maxRecords), keyword);
        }

        public int SelectCount(string Filter)
        {
            Object count = null;
            string keyword = (null == Filter) ? "%" : Filter;
            switch (Config.filterType)
            {
                case FilterType.FUNC:
                    count = ExecuteScalar("SELECT count(*) FROM (" +queryFunc+ ")", keyword);
                    break;
                case FilterType.TAG:
                    count = ExecuteScalar("SELECT count(*) FROM ("+queryTags+")", keyword);
                    break;
                default:
                case FilterType.ALL:
                    count = ExecuteScalar("SELECT count(*) FROM (" + queryAll + ")", keyword);
                    break;
            }

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