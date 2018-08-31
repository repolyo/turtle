using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Turtle;

public partial class MasterPage : System.Web.UI.MasterPage
{
    bool login = false;

    public UserProfile CurrentUser
    {
        get { return (UserProfile)Session["current_user"]; }
        set { Session["current_user"] = value; }
    }

    public string CurrentUserName
    {
        get { return (string)Session["username"]; }
        set { Session["username"] = value; }
    }

    public bool VisibleWhenLoggedIn { get { return login; } set { login = value; } }

    public String Version
    {
        get { return (String)ViewState["version"]; }
        set { ViewState["version"] = value; }
    }

    public string Status
    {
        get { return StatusTxtBox.Text; }
        set { StatusTxtBox.Text = value; }
    }

    public DataTable DataTable
    {
        get { return (DataTable)ViewState["DataTable"]; }
        set { ViewState["DataTable"] = value; }
    }

    void Page_Init(Object sender, EventArgs e)
    {
        this.Version = "v1.0";
        
        this.ApplicationName.Text = Config.ApplicationName;
        this.ApplicationName.ToolTip = Config.ApplicationDescription;
        this.UserName.Text = (null != CurrentUser) ? CurrentUser.DisplayName : "";
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        this.UserName.Visible = (null != CurrentUser);
    }
}
