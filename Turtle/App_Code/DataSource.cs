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
        public const string queryFunc = "SELECT " +
                "   t.TGUID " +
                "  FROM " +
                "   TESTCASE_FUNC t, " +
                "   FUNC f " +
                "  WHERE " +
                "    t.fid = f.fid " +
                "    AND f.FUNC_NAME LIKE '{0}'";

        public const string queryTags = "SELECT " +
                "    tt.TGUID " +
                "   FROM " +
                "    TAGS t, " +
                "    TESTCASE_TAGS tt " +
                "   WHERE " +
                "     t.tid = tt.tag_id " +
                "     AND UPPER(t.TAG_NAME) LIKE UPPER('{0}')";

        public const string queryType = "SELECT " +
                "    t.TGUID " +
                "   FROM " +
                "    TESTCASE t " +
                "   WHERE " +
                "     UPPER(t.TTYPE) = UPPER('{0}')";

        public const string queryAll = "SELECT ROW_NUMBER() OVER (ORDER BY a.CREATE_DATE DESC) AS ROWNO, " +
                "  a.TGUID as TID, " +
                "  a.CREATE_DATE, " +
                "  ' ' as Filter, " +
                "  a.TNAME, " +
                "  a.TTYPE, " +
                "  a.TSIZE, " +
                "  a.TLOC " +
                "FROM TESTCASE a WHERE a.TGUID IN (" + queryFunc + " UNION " + queryTags + " )";

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
            string sql = "SELECT ROW_NUMBER() OVER (ORDER BY a.CREATE_DATE DESC) AS ROWNO, " +
                        "  a.TGUID as TID," +
                        "  a.CREATE_DATE, " +
                        "  ' ' as Filter, " +
                        "  a.TNAME, " +
                        "  a.TTYPE, " +
                        "  a.TSIZE, " +
                        "  a.TLOC " +
                        "FROM TESTCASE a WHERE a.TGUID IN ";
            switch (Config.filterType)
            {
                case FilterType.FUNC:
                    sql += " (" + queryFunc + ")";
                    break;
                case FilterType.TYPE:
                    sql += " (" + queryType + ")";
                    break;
                case FilterType.TAG:
                    sql += " (" + queryTags + ")";
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
                case FilterType.TYPE:
                    count = ExecuteScalar("SELECT count(*) FROM (" + queryType + ")", keyword);
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
            "   t.TGUID as TID, " +
            "   f.* " +
            " FROM " +
            "   TESTCASE t, " +
            "   TESTCASE_FUNC tf, " +
            "   FUNC f " +
            " WHERE " +
            "   t.TGUID = '{0}' " +
            "   AND t.TGUID = tf.TGUID " +
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

    public class TagData : DbConn
    {
        private const string query = "SELECT " +
            "   ROW_NUMBER() OVER (ORDER BY t.CREATE_DATE DESC) AS ROWNO, " +
            "   t.TID, " +
            "   t.TAG_NAME, " +
            "   t.TAG_DESCR " +
            " FROM " +
            "   TAGS t " +
            " WHERE " +
            "   t.TAG_NAME LIKE {0} ";

        public TagData()
        {
            NewConnection(Config.getConnectionString());
        }

        ~TagData()
        {
            Terminate();
        }

        public int SelectCount(string TAG_NAME)
        {
            Object count = ExecuteScalar("SELECT count(*) FROM (" + query + ")",
                (null == TAG_NAME) ? "'%'" : TAG_NAME);
            return int.Parse(count.ToString());
        }

        public DataTable QueryTags(string TAG_NAME, string sortColumns, int startRecord, int maxRecords)
        {
            DataTable table = Query(
                String.Format("SELECT * FROM ({0}) WHERE ROWNO > {1} AND ROWNO <= ({1} + {2})",
                    query, startRecord, maxRecords),
                    (null == TAG_NAME || 0 == TAG_NAME.Length) ? "'%'" : TAG_NAME);

            return table;
        }
        
        public static void UpdateTag(string TAG_NAME, string TAG_DESCR, string TID)
        {
            Update("UPDATE TAGS SET TAG_NAME='{0}', TAG_DESCR='{1}' WHERE TID = {2}",
                TAG_NAME, TAG_DESCR, TID);
        }

        public static void DeleteTag(string TID)
        {
            Update("DELETE FROM TAGS WHERE TID = {0}", TID);
        }

        public static void InsertTag(string TAG_NAME, string TAG_DESCR)
        {
            Update("INSERT INTO TAGS (TAG_NAME, TAG_DESCR) VALUES ('{0}', '{1}')",
                TAG_NAME, TAG_DESCR);
        }
    }
}