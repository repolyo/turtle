using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        DbConn.NewConnection();
        DataTable table = DbConn.GetEmployees();
        GridView1.DataSource = table;
        GridView1.DataBind();
        DbConn.Terminate();
    }


    protected void btnFiltering_Click(object sender, EventArgs e)
    {
        DbConn.NewConnection();
        DataTable table = DbConn.Query("SELECT TNAME, TLOC FROM TESTCASE WHERE UPPER(TNAME) LIKE '{0}' AND ROWNUM < 50",
            txtFilter.Text.ToUpper());
        GridView1.DataSource = table;
        GridView1.DataBind();
        DbConn.Terminate();
    }

    protected void grdData_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        GridView1.PageIndex = e.NewPageIndex;
        DbConn.NewConnection();
        DataTable table = DbConn.Query(
            "SELECT * FROM " + 
            "(SELECT TNAME, TLOC FROM TESTCASE WHERE ROW_NUMBER() OVER (ORDER BY TNAME) AS IDX ) " +
            "WHERE IDX = {0}",
            GridView1.PageIndex * 10);

        GridView1.DataSource = table;
        GridView1.DataBind();
        DbConn.Terminate();
    }
}