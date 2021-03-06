﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Web.Security;
using Turtle;

public partial class Login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Master.CurrentUser = null;
    }

    public void ValidateUser(object sender, EventArgs e)
    {
        UserProfile user = new UserProfile(form_login.UserName);
        String uid = string.Format("uid={0}", user.UserName);
        String basedn = "ou=Employees,o=lexmark";
        String ldapusr = string.Format("{0},{1}", uid, basedn);
        String passwd = form_login.Password;

        try
        {
            string ldap_host = string.Format("{0}:{1}", Config.LdapServer, Config.LdapPort);
            LdapDirectoryIdentifier ldapDirectoryIdentifier = new LdapDirectoryIdentifier(ldap_host, true, false);
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
                    string [] keys = new string[]
                    {
                        "displayname",
                        "lexorgpersonmail"
                    };
                    SearchResultEntry entry = response.Entries[0];
                    foreach (string key in keys)
                    {
                        DirectoryAttribute attr = entry.Attributes[key];
                        user[key] = attr.GetValues(typeof(string)).GetValue(0).ToString();

                        string log = string.Format("{0} = {1}", key, user[key]);
                        Console.WriteLine(log);
                    }

                    Master.VisibleWhenLoggedIn = true;
                    Master.CurrentUserName = form_login.UserName;
                    Session["current_user"] = user;
                    FormsAuthentication.RedirectFromLoginPage(form_login.UserName, form_login.RememberMeSet);
                }
                else
                {
                    throw new Exception("Multiple match for {0}" + user.UserName);
                }
            }
        }
        catch (Exception err)
        {
            form_login.FailureText = err.Message;
        }
    }
}