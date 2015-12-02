using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

public partial class _Default : System.Web.UI.Page
{
    const string sql = "SELECT  *  FROM " +
        "(SELECT ROW_NUMBER() OVER (ORDER BY a.tid) AS ROWNO, " +
        "  a.tid, " +
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
        "    AND UPPER(f.FUNC_NAME) LIKE UPPER('{2}') " +
        "  UNION " +
        "   SELECT " +
        "    tt.tid " +
        "   FROM " +
        "    TAGS t, " +
        "    TESTCASE_TAGS tt " +
        "   WHERE " +
        "     t.tid = tt.tag_id " +
        "     AND UPPER(t.TAG_NAME) LIKE UPPER('{2}') " +
        "  ) a JOIN TESTCASE tc ON a.tid = tc.tid) " +
        "WHERE ROWNO > {0} AND ROWNO <= ({0} + {1})";

    protected void Page_Load(object sender, EventArgs e)
    {
        EmployeesObjectDataSource.SelectParameters["Filter"].DefaultValue = "%";
    }

    protected void btnFiltering_Click(object sender, EventArgs e)
    {
        EmployeesObjectDataSource.SelectParameters["Filter"].DefaultValue = txtFilter.Text;
    }
}