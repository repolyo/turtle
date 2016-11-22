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
        public static int fetchCount = 0;
        public static string querySQL;

        public const string queryFunc = "SELECT UNIQUE" +
                "    t.TGUID " +
                "  FROM " +
                "   TESTCASE_FUNC t _::_, " +
                "   FUNC f " +
                "  WHERE " +
                "    t.fid = f.fid " +
                "    AND f.FUNC_NAME LIKE '{0}'";

        public const string queryTags = "SELECT UNIQUE" +
                "    t.TGUID " +
                "   FROM " +
                "    TAGS tt , " +
                "    TESTCASE_TAGS t _::_ " +
                "   WHERE " +
                "     tt.tid = t.tag_id " +
                "     AND UPPER(tt.TAG_NAME) LIKE UPPER('{0}')";

        public const string queryType = "SELECT UNIQUE" +
                "    t.TGUID " +
                "   FROM " +
                "    TESTCASE t _::_" +
                "   WHERE " +
                "     t.HIDDEN <> 'Y' AND UPPER(t.TTYPE) like UPPER('{0}')";

        public const string queryAll = "SELECT ROW_NUMBER() OVER (ORDER BY a.UPDATE_DATE DESC, a.CREATE_DATE DESC) AS ROWNO, " +
                "  a.TGUID as TID, " +
                "  a.CREATE_DATE, " +
                "  ' ' as Filter, " +
                "  a.TNAME, " +
                "  a.TTYPE, " +
                "  a.TSIZE, " +
                "  a.TLOC " +
                "FROM TESTCASE a WHERE a.HIDDEN <> 'Y' AND a.TGUID IN (" + queryFunc + " UNION " + queryTags + " )";

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

        private string normalizeFilter(string filter)
        {
            string keyword = (null == filter) ? "%" : filter.Trim();
            return keyword.Replace('*', '%').Replace('?', '_');
        }

        // Select testcases. 
        public DataTable QueryTestcases(string Filter, string sortColumns, int startRecord, int maxRecords)
        {
            string sql = "";
            string keyword = normalizeFilter(Filter);

            if ( 0 >= (startRecord + maxRecords) )
            {
                throw new Exception(String.Format("ERROR -- Invalid range: {0} to {1}", startRecord, maxRecords));
            }

            sql = "SELECT ROW_NUMBER() OVER (ORDER BY a.UPDATE_DATE DESC NULLS LAST, a.CREATE_DATE DESC) AS ROWNO, " +
                        "  a.TGUID as TID," +
                        "  a.CREATE_DATE, " +
                        "  '" + keyword + "' as Filter, " +
                        "  a.TNAME, " +
                        "  a.TTYPE, " +
                        "  a.TSIZE, " +
                        "  a.TLOC " +
                        "FROM TESTCASE a WHERE a.HIDDEN <> 'Y' AND a.TGUID IN ";
            try
            {
                switch (Config.filterType)
                {
                    case FilterType.ALL:
                    case FilterType.FUNC:
                    default:
                        sql += " (" + queryFunc + ")";
                        break;
                    case FilterType.TYPE:
                        sql += " (" + queryType + ")";
                        break;
                    case FilterType.TAG:
                        sql += " (" + queryTags + ")";
                        break;
                }

                if (0 < Config.personaId)
                {
                    sql = sql.Replace("_::_", " JOIN TESTCASE_RUN r ON r.TGUID = t.TGUID AND r.pid like " + Config.personaId);
                }
                else {
                    sql = sql.Replace("_::_", "");
                }

                querySQL = String.Format("SELECT * FROM ({0}) WHERE ROWNO > {1} AND ROWNO <= ({1} + {2})",
                                    sql, startRecord, maxRecords);
                return Query(querySQL, keyword);
            }
            catch (Exception e)
            {
                throw new Exception(sql, e);
            }
            finally
            {
                //if (Config.personaId > 1)
                // {
                //    throw new Exception(sql);
                //}
            }
        }

        public int SelectCount(string Filter)
        {
            Object count = null;
            string keyword = normalizeFilter(Filter);
            string sql = "";
            try
            {
                switch (Config.filterType)
                {
                    case FilterType.FUNC:
                        sql = String.Format("SELECT count(*) FROM ({0})", queryFunc);
                        break;
                    case FilterType.TAG:
                        sql = String.Format("SELECT count(*) FROM ({0})", queryTags);
                        break;
                    case FilterType.TYPE:
                        sql = String.Format("SELECT count(*) FROM ({0})", queryType);
                        break;
                    default:
                    case FilterType.ALL:
                        sql = String.Format("SELECT count(*) FROM ({0})", queryAll);
                        break;
                }

                if (0 < Config.personaId)
                {
                    sql = sql.Replace("_::_", " JOIN TESTCASE_RUN r ON r.TGUID = t.TGUID AND r.pid like " + Config.personaId);
                }
                else
                {
                    sql = sql.Replace("_::_", "");
                }

                count = ExecuteScalar(sql, keyword);
                fetchCount = int.Parse(count.ToString());
            }
            catch (Exception e)
            {
                throw new Exception(sql, e);
            }
            finally
            {
                if (0 == fetchCount)
                {
                    throw new Exception(sql);
                }
            }

            return fetchCount;
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
            "   AND tf.fid = f.fid " +
            "   AND tf.pid = {1} ";

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
            Exception e = null;
            string sqlQuery = "SELECT count(*) FROM (" + query + ")";
            try
            {
                Object count = ExecuteScalar(sqlQuery, (null == TID) ? "%" : TID, Config.personaId);
                TestcaseProfileData.fetchCount = int.Parse(count.ToString());
            }
            catch (Exception ex)
            {
                e = ex;
            }
            finally
            {
                if (0 == TestcaseProfileData.fetchCount)
                {
                    throw new Exception(sqlQuery, e);
                }
            }
            return TestcaseProfileData.fetchCount;
        }

        public DataTable QueryFunctions(string TID, string sortColumns, int startRecord, int maxRecords)
        {
            TestcaseProfileData.querySQL = String.Format("SELECT * FROM ({0}) WHERE ROWNO > {1} AND ROWNO <= ({1} + {2})",
                    query, startRecord, maxRecords);

            return Query(TestcaseProfileData.querySQL, (null == TID) ? "%" : TID, Config.personaId);
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
            TestcaseProfileData.fetchCount = int.Parse(count.ToString());
            return TestcaseProfileData.fetchCount;
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