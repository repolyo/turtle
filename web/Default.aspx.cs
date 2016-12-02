using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Samples.AspNet.ObjectDataSource;
using System.IO;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string filter = Request.QueryString["Filter"];
        if (null == filter) {
            filter = "xxx";
        }
        else {
            txtFilter.Text = filter;
        }

        Config.personaId = 0;
        Config.debug = (null != Request.QueryString["debug"]) ? true : false;
        TestcaseProfileData.fetchCount = 0;
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

    protected void btnExportToExcel_Click(object sender, EventArgs e)
    {
        Samples.AspNet.ObjectDataSource.TestcaseProfileData tcpd = new Samples.AspNet.ObjectDataSource.TestcaseProfileData();
        string path = @"D:\Turtle-export.csv";
        var lines = new List<string>();

        string[] columnNames = tcpd.QueryTestcases(txtFilter.Text, null, 0, 1000).Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();

        var header = string.Join(",", columnNames);
        lines.Add(header);

        var valueLines = tcpd.QueryTestcases(txtFilter.Text, null, 0, 1000).AsEnumerable().Select(row => string.Join(",", row.ItemArray));
        lines.AddRange(valueLines);

        File.WriteAllLines(path, lines);
    }

    protected void ExportToExcel(object sender, EventArgs e)
    {
        try
        {
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=Testcases.csv");
            Response.Charset = "";
            Response.ContentType = "text/csv";
            using (StringWriter sw = new StringWriter())
            {
                HtmlTextWriter hw = new HtmlTextWriter(sw);
                Samples.AspNet.ObjectDataSource.TestcaseProfileData tcpd = new Samples.AspNet.ObjectDataSource.TestcaseProfileData();

                var lines = new List<string>();
                int fetchCount = int.Parse(totalLbl.Text);
                DataTable dt = (fetchCount > 0) ? tcpd.QueryTestcases(txtFilter.Text, null, 0, fetchCount) : null;
                if (null != dt)
                {
                    string[] columnNames = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
                    var header = string.Join(",", columnNames);
                    lines.Add(header);

                    var valueLines = tcpd.QueryTestcases(
                        txtFilter.Text, null, 0, fetchCount).AsEnumerable().Select(row => string.Join(",", row.ItemArray));
                    lines.AddRange(valueLines);

                    foreach (string s in lines)
                    {
                        sw.WriteLine(s);
                    }
                }
                else 
                {
                    sw.WriteLine("No results!");
                }

                //style to format numbers to string
                //string style = @"<style> .textmode { } </style>";
                //Response.Write(style);
                //Response.Output.Write(sw.ToString());

                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(sw.ToString());
                Response.OutputStream.Write(bytes, 0, bytes.Length);

                Response.Flush();
                Response.End();
            }
        }
        catch (Exception ex)
        {
            querySQL.Text = totalLbl.Text + " = " + TestcaseProfileData.querySQL;
            throw new Exception(querySQL.Text, ex);
        }
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
            Config.personaId = int.Parse(PersonaCbx.SelectedItem.Value);
        }
        catch (Exception ex)
        {
            throw new Exception(ddlFilter.SelectedValue, ex);
        }
    }

    protected void GridVIew_OnDataBound(object sender, EventArgs e) {
        Config.filterType = ParseEnum<FilterType>(ddlFilter.SelectedValue);
        switch (Config.filterType)
        {
            case FilterType.TAG:
                GridView1.EmptyDataText = String.Format("No testcase found for tag: {0}",
                    TestcaseProfileData.filter);
                break;
            case FilterType.TYPE:
                GridView1.EmptyDataText = String.Format("No testcase found testcase type: {0}",
                    TestcaseProfileData.filter);
                break;
            case FilterType.FILE:
                GridView1.EmptyDataText = String.Format("No testcase found for filename: {0}",
                    TestcaseProfileData.filter);
                break;
            case FilterType.FUNC:
                GridView1.EmptyDataText = String.Format("No testcase found for function: {0}",
                    TestcaseProfileData.filter);
                break;
            default:
                break;
        }

        totalLbl.Text = string.Format("{0}", TestcaseProfileData.fetchCount);
        querySQL.Text = string.Empty;
        if ( Config.debug ) querySQL.Text = TestcaseProfileData.querySQL;
    }
}
