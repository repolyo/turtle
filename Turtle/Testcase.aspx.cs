﻿using System;
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

        List<string> checksums = new List<string>();
        table = DbConn.Query(
                String.Format("SELECT CHECKSUM FROM TESTCASE_CHECKSUM WHERE TID = {0} ORDER BY PAGE_NO", tid));
        if (null != table) {
            foreach (DataRow row in table.Rows) {
                checksums.Add(row["CHECKSUM"].ToString());
            }
        }
        tcCS.Text = String.Format("Checksums:  {0}", string.Join(", ", checksums.ToArray()) );
        DbConn.Terminate();
    }
}