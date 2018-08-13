using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Web.Security;

public partial class Login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Session["username"] = null;
    }

    public void ValidateUser(object sender, EventArgs e)
    {
        UserProfile user = new UserProfile(Login1.UserName);
        String uid = string.Format("uid={0}", user.UserName);
        String basedn = "ou=Employees,o=lexmark";
        String ldapusr = string.Format("{0},{1}", uid, basedn);
        String passwd = Login1.Password;

        try
        {
            LdapDirectoryIdentifier ldapDirectoryIdentifier = new LdapDirectoryIdentifier("dirservices.lexmark.com:389", true, false);
            NetworkCredential credentials = new NetworkCredential(ldapusr, passwd);
            using (LdapConnection ldapConnection = new LdapConnection(ldapDirectoryIdentifier, credentials, AuthType.Basic))
            {
                ldapConnection.SessionOptions.SecureSocketLayer = false;
                ldapConnection.SessionOptions.ProtocolVersion = 3; // LDAP_OPT_PROTOCOL_VERSION
                ldapConnection.Bind();

                // distinguished name of the object 
                // at which to start the search.

                SearchRequest searchRequest = new SearchRequest(basedn, string.Format("({0})", uid), System.DirectoryServices.Protocols.SearchScope.Subtree, null);
                SearchResponse response = (SearchResponse)ldapConnection.SendRequest(searchRequest);

                if (response.Entries.Count == 1)
                {
                    SearchResultEntry entry = response.Entries[0];
                    foreach (string key in entry.Attributes.AttributeNames)
                    {
                        DirectoryAttribute attr = entry.Attributes[key];
                        user[key] = attr.GetValues(typeof(string)).GetValue(0).ToString();

                        string log = string.Format("{0} = {1}", key, user[key]);
                        Console.WriteLine(log);
                    }

                    Master.VisibleWhenLoggedIn = true;
                    Session["username"] = Login1.UserName;
                    FormsAuthentication.RedirectFromLoginPage(Login1.UserName, Login1.RememberMeSet);
                }
                else
                {
                    throw new Exception("Multiple match for {0}" + user.UserName);
                }
            }
        }
        catch (Exception err)
        {
            Login1.FailureText = err.Message;
        }
    }
}