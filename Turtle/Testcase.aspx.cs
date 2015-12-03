using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections.Specialized;
using System.Web.Configuration;

public partial class Testcase : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string tid = Request.QueryString["TID"]; ;
        TestcaseDS.SelectParameters["TID"].DefaultValue = tid;
        QueryDetails(tid);
    }

    protected void QueryDetails(string tid)
    {
        DbConn.NewConnection(Config.getConnectionString());
        DataTable table = DbConn.Query(
                String.Format("SELECT * FROM TESTCASE WHERE TID = {0}", tid));
        foreach(DataRow row in table.Rows)
        {
            tcName.Text = String.Format("Testcase: {0}", row["TNAME"]);
            tcLoc.Text = String.Format("Location: {0}", row["TLOC"]);
            break;
        }

        DbConn.Terminate();
    }
}