﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Testcases_Testcases : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        TObjectDataSource.SelectParameters["Filter"].DefaultValue = "*";
    }

    protected void GridVIew_OnDataBound(object sender, EventArgs e)
    {
    }
}