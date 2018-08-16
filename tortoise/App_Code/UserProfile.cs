using System;
using System.Collections;
using System.Linq;
using System.Web;

namespace Turtle
{
    /// <summary>
    /// Summary description for UserProfile
    /// </summary>
    public class UserProfile : Hashtable
    {

        public UserProfile(string name)
        {
            UserName = name;
        }

        // Declare a Code property of type string:
        public string UserName
        {
            get { return this["username"].ToString(); }
            set { this["username"] = value; }
        }

        public string IdNumber
        {
            get { return this["lexcustuid"].ToString(); }
            set { this["lexcustuid"] = value; }
        }

        public string DisplayName
        {
            get { return this["displayname"].ToString(); }
            set { this["displayname"] = value; }
        }

        public string GivenName
        {
            get { return this["givenname"].ToString(); }
            set { this["givenname"] = value; }
        }

        public string Email
        {
            get { return this["lexorgpersonmail"].ToString(); }
            set { this["lexorgpersonmail"] = value; }
        }
        public string Mobile
        {
            get { return this["mobile"].ToString(); }
            set { this["mobile"] = value; }
        }

    }
}