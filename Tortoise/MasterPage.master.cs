using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class MasterPage : System.Web.UI.MasterPage
{
    bool login = false;

    public bool VisibleWhenLoggedIn { get { return login; } set { login = value; } }

    public String Version
    {
        get { return (String)ViewState["version"]; }
        set { ViewState["version"] = value; }
    }

    void Page_Init(Object sender, EventArgs e)
    {
        this.Version = "v1.0";
        this.ApplicationName.Text = "Turtle v1.0";
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        bool visible = !string.IsNullOrEmpty((string)Session["username"]);
        this.TreeView1.Visible = visible;
    }
}
