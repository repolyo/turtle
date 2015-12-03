﻿using System;
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
        string filter = Request.QueryString["Filter"];
        if (null == filter) {
            filter = "%";
        }
        else {
            txtFilter.Text = filter;
        }
        TObjectDataSource.SelectParameters["Filter"].DefaultValue = filter;
    }

    protected void btnFiltering_Click(object sender, EventArgs e)
    {
        TObjectDataSource.SelectParameters["Filter"].DefaultValue = txtFilter.Text;
    }
}