using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections.Specialized;
using System.Web.Configuration;
using Samples.AspNet.ObjectDataSource;

public partial class Testcase : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Config.debug = (null != Request.QueryString["debug"]) ? true : Config.debug;
        string tid = Request.QueryString["TID"];
        TestcaseDS.SelectParameters["TID"].DefaultValue = tid;
        
        QueryDetails(tid);
    }

    protected void QueryDetails(string tid)
    {
        DbConn.NewConnection(Config.getConnectionString());
        DataTable table = DbConn.Query(String.Format(
            "SELECT TO_CHAR (START_TIME, 'MM/DD/YY') SDATE, " +
            "(END_TIME - START_TIME) ELAPSE FROM TESTCASE_RUN WHERE TGUID = '{0}' ORDER BY START_TIME DESC", 
            tid));
        if (null != table)
        {
            foreach (DataRow row in table.Rows)
            {
                tcTime.Text = String.Format("Last Run: {0}, Took: {1}", row["SDATE"], row["ELAPSE"]);
                break;
            }
        }

        table = DbConn.Query(String.Format("SELECT * FROM TESTCASE WHERE TGUID = '{0}'", tid));
        if (null != table)
        {
            foreach (DataRow row in table.Rows)
            {
                tcName.Text = String.Format("Testcase: {0}", row["TNAME"]);
                tcLoc.Text = String.Format("Location: {0}", row["TLOC"]);
                break;
            }
        }
        List<string> checksums = new List<string>();
        table = DbConn.Query(
                String.Format("SELECT CHECKSUM FROM TESTCASE_CHECKSUM WHERE TGUID = '{0}' and PID=" + 
                    Config.personaId + " ORDER BY PAGE_NO", tid));
        if (null != table) {
            foreach (DataRow row in table.Rows) {
                checksums.Add(row["CHECKSUM"].ToString());
            }
        }
        tcCS.Text = String.Format("Checksums:  {0}", string.Join(", ", checksums.ToArray()) );
        DbConn.Terminate();
    }

    protected void GridVIew_OnDataBound(object sender, EventArgs e)
    {
        querySQL.Text = string.Empty;
        if (Config.debug) querySQL.Text = TestcaseProfileData.querySQL;
    }
}