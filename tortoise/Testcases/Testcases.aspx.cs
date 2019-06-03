using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Testcases_Testcases : System.Web.UI.Page
{
    static string FILTER = "Filter";

    public static T ParseEnum<T>(string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    private string getFilter ()
    {
        string filter = txtFilter.Text.Replace('*', '%').Replace('?', '_');
        ViewState[FILTER] = filter;

        return filter;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string arg = Request.QueryString[FILTER];
        if (!string.IsNullOrEmpty(arg)) {
            PropertyInfo isreadonly = typeof(System.Collections.Specialized.NameValueCollection).GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);

            txtFilter.Text = arg;
            ViewState[FILTER] = txtFilter.Text;
            TObjectDataSource.SelectParameters[FILTER].DefaultValue = txtFilter.Text;

            // make collection editable
            isreadonly.SetValue(this.Request.QueryString, false, null);
            // remove
            this.Request.QueryString.Remove(FILTER);
        }
    }

    protected void GridVIew_OnDataBound(object sender, EventArgs e)
    {
        Console.WriteLine(txtFilter.Text);
        Console.WriteLine(TObjectDataSource.SelectParameters[FILTER].DefaultValue);

    }

    protected void btnFiltering_Click(object sender, EventArgs e)
    {
        TObjectDataSource.SelectParameters[FILTER].DefaultValue = getFilter ();
    }

    protected void drp1_SelectedIndexChanged(object sender, EventArgs e)
    {
        Console.WriteLine("SelectedIndex = " + ddlFilter.SelectedIndex
            + ", SelectedValue=" + ddlFilter.SelectedValue);
        Config.filterType = ParseEnum<FilterType>(ddlFilter.SelectedValue);
    }

    protected void Skip_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox check = ((CheckBox)sender);
        
        TESTCASE_VIEW tbl = new TESTCASE_VIEW();
      
        tbl.update (String.Format ("UPDATE TESTCASE SET HIDDEN='{0}' WHERE TGUID={1}",
            check.Checked ? 'Y' : 'N',
            check.ToolTip));

        Console.WriteLine("SelectedIndex = " + sender.ToString ()
            + ", SelectedValue=" + e.ToString ());
    }

    protected void TObjectDataSource_OnSelected(object sender, ObjectDataSourceStatusEventArgs e)
    {
        if (null == e.Exception && null != e.ReturnValue)
        {
            Type type = e.ReturnValue.GetType();
            if (type == typeof(TESTCASE_VIEW) && !string.IsNullOrEmpty (txtFilter.Text))
            {
                TESTCASE_VIEW tbl = (TESTCASE_VIEW)e.ReturnValue;
                Count.Text = "" + tbl.SelectCount(txtFilter.Text);
            }

            //Count.Text = "" + ((TESTCASE_CHECKSUMS_VIEW)e.ReturnValue).TotalCount;
            //Count.Text = "" + e.ReturnValue;
        }
    }

    DataTable GetDataTable(GridView dtg)
    {
        DataTable dt = new DataTable();

        // add the columns to the datatable            
        if (dtg.HeaderRow != null)
        {

            for (int i = 0; i < dtg.HeaderRow.Cells.Count; i++)
            {
                dt.Columns.Add(dtg.HeaderRow.Cells[i].Text);
            }
        }

        //  add each of the data rows to the table
        foreach (GridViewRow row in dtg.Rows)
        {
            DataRow dr;
            dr = dt.NewRow();

            for (int i = 0; i < row.Cells.Count; i++)
            {
                dr[i] = row.Cells[i].Text.Replace(" ", "");
            }
            dt.Rows.Add(dr);
        }
        return dt;
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
                TESTCASE tcdb = new TESTCASE();

                var lines = new List<string>();
                
                int fetchCount = tcdb.SelectCount(txtFilter.Text);
                DataTable dt = (fetchCount > 0) ? tcdb.QueryTestcases (txtFilter.Text, null, 0, fetchCount) : null;

                if (null != dt)
                {
                    string[] columnNames = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();
                    var header = string.Join(",", columnNames);
                    lines.Add(header);

                    var valueLines = dt.AsEnumerable().Select(row => string.Join(",", row.ItemArray));
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
            //querySQL.Text = totalLbl.Text + " = " + TestcaseProfileData.querySQL;
            //throw new Exception(querySQL.Text, ex);
            throw ex;
        }
    }
}