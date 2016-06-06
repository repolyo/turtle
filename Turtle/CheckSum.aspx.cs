using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Samples.AspNet.ObjectDataSource;
using System.Data;

public partial class CheckSum : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string func = Request.QueryString["func"];
        string sql = "SELECT ROW_NUMBER() OVER (ORDER BY a.CREATE_DATE DESC) AS ROWNO, " +
                    "  a.TLOC, " +
                    "  (select listagg(CHECKSUM, ',') within group (order by PAGE_NO) from TESTCASE_CHECKSUM where TESTCASE_CHECKSUM.TGUID=a.TGUID) as CHECKSUM" +
                    " FROM TESTCASE a WHERE a.TGUID IN (" + TestcaseProfileData.queryFunc + ")";

        DbConn.NewConnection(Config.getConnectionString());
        DataTable tbl = DbConn.Query(String.Format(
            "SELECT * FROM ({0}) WHERE ROWNO > {1} AND ROWNO <= ({1} + {2})",
            sql, 0, 1000), func == null ? "%" : func);
        if (null != tbl) foreach (DataRow row in tbl.Rows)
        {
            sampleText.Text += String.Format("{0} {1}\n", row["TLOC"], row["CHECKSUM"]);
        }
        DbConn.Terminate();
    }
}