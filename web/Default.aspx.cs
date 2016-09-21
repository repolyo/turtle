using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Samples.AspNet.ObjectDataSource;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string filter = Request.QueryString["Filter"];
        if (null == filter) {
            filter = "%";
        }
        else {
            txtFilter.Text = filter;
        }
        
        TObjectDataSource.SelectParameters["Filter"].DefaultValue = filter;
        DbConn.NewConnection(Config.getConnectionString());
        //totalLbl.Text = "Testcases: " + DbConn.ExecuteScalar("SELECT count(*) FROM TESTCASE");
        DbConn.Terminate();
    }

    protected void btnFiltering_Click(object sender, EventArgs e)
    {
        totalLbl.Text = "Testcases: ";
        TObjectDataSource.SelectParameters["Filter"].DefaultValue = txtFilter.Text;
    }

    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    protected void drp1_SelectedIndexChanged(object sender, EventArgs e)
    {
        Console.WriteLine("SelectedIndex = " + ddlFilter.SelectedIndex
            + ", SelectedValue=" + ddlFilter.SelectedValue);
        Config.filterType = ParseEnum<FilterType>(ddlFilter.SelectedValue);
       // ((TestcaseProfileData)TObjectDataSource).Type = type;
    }

    protected void drp2_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            Console.WriteLine("SelectedIndex = " + PersonaCbx.SelectedIndex
                + ", SelectedValue=" + PersonaCbx.SelectedValue);
            Config.personaId = int.Parse(PersonaCbx.SelectedValue);
        }
        catch (Exception ex)
        {
            throw new Exception(ddlFilter.SelectedValue, ex);
        }
    }

    protected void GridVIew_OnDataBound(object sender, EventArgs e) {
        totalLbl.Text = "Testcases: " + TestcaseProfileData.fetchCount;
    }
}