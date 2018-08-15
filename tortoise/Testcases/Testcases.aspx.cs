using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Testcases_Testcases : System.Web.UI.Page
{
    protected void GridVIew_OnDataBound(object sender, EventArgs e)
    {
        //DataTable dt = (DataTable)GridView1.DataSource;
        //Session["DataTable"] = sender;
        //TObjectDataSource.SelectParameters["Filter"].DefaultValue = txtFilter.Text;
        Console.WriteLine(txtFilter.Text);
        Console.WriteLine(TObjectDataSource.SelectParameters["Filter"].DefaultValue);
    }

    protected void btnFiltering_Click(object sender, EventArgs e)
    {
        //totalLbl.Text = "Testcases: ";
        Session["Filter"] = txtFilter.Text;
        TObjectDataSource.SelectParameters["Filter"].DefaultValue = txtFilter.Text;
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