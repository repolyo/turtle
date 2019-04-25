using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Codes_Files : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        TObjectDataSource.SelectParameters["Filter"].DefaultValue = "*";
        ViewState["Filter"] = TObjectDataSource.SelectParameters["Filter"].DefaultValue;
    }

    protected void GridVIew_OnDataBound(object sender, EventArgs e)
    {
        Console.WriteLine(TObjectDataSource.SelectParameters["Filter"].DefaultValue);

    }

    protected void Files_OnSelected(object sender, ObjectDataSourceStatusEventArgs e)
    {
        if (null == e.Exception && null != e.ReturnValue)
        {
            Type type = e.ReturnValue.GetType();
            if (type == typeof(FUNC))
            {
                FUNC tbl = (FUNC)e.ReturnValue;
                Count.Text = "" + tbl.SelectCount("*");
            }
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

        Master.Status = "SQL: The file could not be uploaded. The following error occured: ";
        return dt;
    }


    protected void AjaxFileUpload1_UploadComplete(object sender, AjaxControlToolkit.AjaxFileUploadEventArgs e)
    {
        bool ret = false;
        string filePath = Config.GetTempPath + e.FileName;
        try
        {
            FUNC tbl = new FUNC();
           // AjaxFileUpload1.SaveAs(filePath);

            ret = tbl.update_func_mappings (Master.CurrentUserName, filePath);
        }
        catch (Exception ex)
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine("Message :" + ex.Message + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                   "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
            }
        }
    }

    protected void UploadButton_Click(object sender, EventArgs e)
    {
        if (FileUploadControl.HasFile)
        {
            try
            {
                FUNC tbl = new FUNC();
                string filename = Path.GetFileName(FileUploadControl.FileName);
                string fullpath = Config.GetTempPath + filename;
                FileUploadControl.SaveAs(fullpath);
                //StatusLabel.Text = "Upload status: File uploaded!";
                tbl.update_func_mappings(Master.CurrentUserName, fullpath);
            }
            catch (Exception ex)
            {
                //StatusLabel.Text = "Upload status: The file could not be uploaded. The following error occured: " + ex.Message;
                throw ex;
            }
        }
    }
}