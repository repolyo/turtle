using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using System.Web.Configuration;

public enum FilterType
{
    ALL = 1,
    FUNC = 2,
    TAG = 3,
    TYPE = 4,
    FILE = 5,
    SIZE
}

/// <summary>
/// Summary description for Class1
/// </summary>
public static class Config
{
    private static string conn_str = null;
    public static FilterType filterType = FilterType.ALL;
    public static int personaId = 5; // sim-atlantis
    public static bool debug = false;
    private static readonly string app_name;
    private static readonly string app_descr;
    private static NameValueCollection settings = null;

    // Static constructor is called at most one time, before any
    // instance constructor is invoked or member is accessed.
    static Config()
    {
        settings = WebConfigurationManager.AppSettings;
        app_name = settings["app_name"];
        app_descr = settings["app_descr"];
    }

    public static string getConnectionString() {
        if (null == conn_str) {
            // Initialize data source. Use "Northwind" connection string from configuration.
             if (settings.Count <= 0) {
                 throw new Exception("A connection string named 'Oracle DB' with a valid connection string " +
                                     "must exist in the <appSettings> configuration section for the application.");
             }

             personaId = int.Parse(settings["persona_id"]);

             conn_str = String.Format(
                "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT=1521)))" +
                "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={1})));" +
                "User Id={2};Password={3};", 
                    settings["db_host"],
                    settings["svc_name"],
                    settings["db_user"],
                    settings["db_pass"]);
        }
        return conn_str;
    }

    public static string ApplicationName
    {
        get { return app_name; }
    }

    public static string ApplicationDescription
    {
        get { return app_descr; }
    }
}