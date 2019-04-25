using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Codes_Coverage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        CoverageDataSource.SelectParameters["Filter"].DefaultValue = "*";
        ViewState["Filter"] = CoverageDataSource.SelectParameters["Filter"].DefaultValue;
    }

    protected void Codes_OnDataBound(object sender, EventArgs e)
    {
        Console.WriteLine(CoverageDataSource.SelectParameters["Filter"].DefaultValue);

    }
}