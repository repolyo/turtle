using System;
using System.Collections.Generic;
using System.Data;
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
    public DataTable DataTable
    {
        get { return (DataTable)ViewState["DataTable"]; }
        set { ViewState["DataTable"] = value; }
    }

    void Page_Init(Object sender, EventArgs e)
    {
        UserProfile profile = (UserProfile)Session["user"];
        this.Version = "v1.0";
        
        this.ApplicationName.Text = Config.ApplicationName;
        this.ApplicationName.ToolTip = Config.ApplicationDescription;

        if (null != profile) {
            this.UserName.Text = profile.DisplayName;
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        UserProfile profile = (UserProfile)Session["user"];
        bool visible = !string.IsNullOrEmpty((string)Session["username"]);
        this.TreeView1.Visible = visible;
    }
}
