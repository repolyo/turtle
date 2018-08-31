using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Testcases_Checksums : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        update_checksum_btn.Enabled = (null != Master.CurrentUser);
        update_checksum_btn.ToolTip = update_checksum_btn.Enabled
            ? "Update checksums..."
            : "Login to udpate checkums...";
    }

    protected void ChecksumGrid_OnDataBound(object sender, EventArgs e)
    {
        //Console.WriteLine(txtFilter.Text);
        //Console.WriteLine(TObjectDataSource.SelectParameters["Filter"].DefaultValue);
        //Count.Text = ChecksumDS.SelectCountMethod;
    }

    protected void PersonaCbx_SelectedIndexChanged(object sender, EventArgs e)
    {
        ViewState["Filter"] = PersonaCbx.SelectedValue;
        ChecksumDS.SelectParameters["Filter"].DefaultValue = PersonaCbx.SelectedValue;
    }

    protected void ChecksumDS_OnSelected (object sender, ObjectDataSourceStatusEventArgs e)
    {
        if (null == e.Exception && null != e.ReturnValue)
        {
            Type type = e.ReturnValue.GetType();
            if (type == typeof(TESTCASE_CHECKSUMS_VIEW) && !string.IsNullOrEmpty(PersonaCbx.SelectedValue))
            {
                TESTCASE_CHECKSUMS_VIEW tbl = (TESTCASE_CHECKSUMS_VIEW)e.ReturnValue;
                Count.Text = "" + tbl.SelectCount(PersonaCbx.SelectedValue);
            }
        }
    }

    protected void AjaxFileUpload1_UploadComplete(object sender, AjaxControlToolkit.AjaxFileUploadEventArgs e)
    {
        bool ret = false;
        string filePath = Config.GetTempPath + e.FileName;
        try
        {
            TESTCASE_CHECKSUMS_VIEW tbl = new TESTCASE_CHECKSUMS_VIEW();
            AjaxFileUpload1.SaveAs(filePath);

            ret = tbl.update_checksums(Master.CurrentUserName, filePath);
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
}