using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
    void Page_Load(Object sender, EventArgs e)
    {
        Version.Text = "Turtle " + Master.Version;
    }
}