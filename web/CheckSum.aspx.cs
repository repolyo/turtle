using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Samples.AspNet.ObjectDataSource;
using System.Data;
using System.Text;

public partial class CheckSum : System.Web.UI.Page
{


    String _testcases = "# testcase : #checksum\r\n";


    protected string generateQuerySQL(string funcList, string pid)
    {
        StringBuilder testcaseByFunc = new StringBuilder("SELECT UNIQUE" +
        "   t.TGUID, listagg(cs.CHECKSUM, ',') within group (order by cs.PAGE_NO) as CHECKSUM" +
        "  FROM " +
        "   TESTCASE_FUNC t, " +
        "   FUNC f, " +
        "   TESTCASE_CHECKSUM cs " +
        "  WHERE " +
        "    t.tguid = cs.tguid AND t.fid = f.fid AND cs.pid = " + pid +
        "    AND UPPER(f.FUNC_NAME) IN ( ");
        if (funcList == null) funcList = "%";

        string[] funcs = funcList.Split(',');
        foreach (string func in funcs)
        {
            if (func == "main") continue;
            testcaseByFunc.Append(String.Format("UPPER('{0}'), ", func));
        }
        testcaseByFunc.Append(" '__end__' ) GROUP BY t.tguid");

        return testcaseByFunc.ToString();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string debug = Request.QueryString["debug"];
        string func = Request.QueryString["func"];
        string persona = Request.QueryString["persona"];
        string platformId = (null != Request.QueryString["pid"]) ? Request.QueryString["pid"] : "1";
        string filter = Request.QueryString["filter"];
        string fetchCount = (null != Request.QueryString["fetch"]) ? Request.QueryString["fetch"] : "50";

        if (null == filter || filter.Length == 0)
        {
            filter = "/m/tcases/futures/next/wip/";
        }

        if (null != persona && persona.Length > 0)
        {
            platformId = String.Format("(select PID from PLATFORM where PERSONA='{0}')", persona);
        }

        string sql = String.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY a.CREATE_DATE DESC) AS ROWNO, " +
                    "  a.TLOC, " +
                    "  b.CHECKSUM " +
                    "FROM TESTCASE a, " + "(" + generateQuerySQL(func, platformId) + ") b " +
                    "WHERE a.TGUID=b.TGUID AND a.HIDDEN <> 'Y' AND a.TLOC LIKE '%" + filter + "%' ) WHERE ROWNO > {0} AND ROWNO <= ({0} + {1})", 0, fetchCount);
        _testcases = "# testcase : #checksum\r\n";

        DbConn.NewConnection(Config.getConnectionString());
        DataTable tbl = DbConn.Query(sql);
        if (null != tbl) foreach (DataRow row in tbl.Rows)
        {
            string checksum = row["CHECKSUM"].ToString().Trim();
            if (checksum.Length == 0) continue;
            string tcase = String.Format("{0} : {1}\r\n", row["TLOC"], checksum);
            _testcases += tcase.Replace("/m/tcases/futures/next/wip/", "");
        }
        DbConn.Terminate();

        //sampleText.Text = _testcases;
        if (debug != null)
        {
            Response.Write("-- SQL Query: \n<br/> " + sql + ";\n<br/><br/>");
        }
    }

    protected String GetTestcases()
    {
        return _testcases;
    }
}