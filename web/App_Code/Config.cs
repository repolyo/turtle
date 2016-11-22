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
    TYPE
}

/// <summary>
/// Summary description for Class1
/// </summary>
public class Config
{
    private static string conn_str = null;
    public static FilterType filterType = FilterType.ALL;
    public static int personaId = 5; // sim-atlantis
    public static bool debug = false;

    public static string getConnectionString() {
        if (null == conn_str) {
            // Initialize data source. Use "Northwind" connection string from configuration.
             NameValueCollection settings = WebConfigurationManager.AppSettings;
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
}