using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Text;
using System.Data;
using System.Web.Configuration;
using System.Collections.Specialized;

[WebService(Namespace = "https://turtle@lrdc.lexmark.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]

public class Checksum : System.Web.Services.WebService
{
    static string downloadFolder = "C:\\Users\\Public\\Downloads\\";

    // http://localhost:49163/ChecksumSVC/Checksum.asmx?WSDL
    public Checksum () {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    private static void writeResponse(HttpResponse response, string text)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text + "\r\n");
        response.OutputStream.Write(bytes, 0, bytes.Length);
    }

    protected string generateQuerySQL(string filter, string pid)
    {
        string funcList = (null == filter) ? "%" : filter.Trim();
        funcList = funcList.Replace('*', '%').Replace('?', '_');

        StringBuilder testcaseByFunc = new StringBuilder();
        string[] funcs = funcList.Split(',');
        foreach (string func in funcs)
        {
            testcaseByFunc.Append(String.Format("UPPER('{0}'), ", func));
        }
        return testcaseByFunc.ToString ();
    }


    [WebMethod]
    public bool SaveDocument(Byte[] docbinaryarray, string docname)
    {
        string strdocPath;
        strdocPath = downloadFolder + docname;
        FileStream objfilestream = new FileStream(strdocPath, FileMode.Create, FileAccess.ReadWrite);
        objfilestream.Write(docbinaryarray, 0, docbinaryarray.Length);
        objfilestream.Close();

        return true;
    }

    // http://localhost:49163/ChecksumSVC/Checksum.asmx/GetDocument?DocumentName=DB_UNLOCK.txt
    [WebMethod]
    public Byte[] GetDocument(string DocumentName)
    {
        string author = "Checksum generator v1.0 <tanch@lexmark.com>\n\n\n";
        string strdocPath;
        strdocPath = downloadFolder + DocumentName;

        HttpResponse response = Context.Response;

        response.Clear();
        response.Buffer = true;

        writeResponse(response, author);

        FileStream objfilestream = new FileStream(strdocPath, FileMode.Open, FileAccess.Read);
        int len = (int)objfilestream.Length;
        Byte[] documentcontents = new Byte[len];
        objfilestream.Read(documentcontents, 0, len);
        objfilestream.Close();

        response.OutputStream.Write(documentcontents, 0, len);
        response.Flush();
        response.End();
        return documentcontents;
    }


    [WebMethod]
    public void Get(string persona, string func, int dpi = 600, int fetch = 50, string filter = null, bool debug = false)
    {
        Fetch("firmware-6", persona, func, dpi, fetch, filter, debug);
    }

    // sample usage:
    //  http://localhost/testcases/Checksum.asmx/Get?persona=sim-atlantis&func=pdflex_add_object&dpi=600&fetch=10&filter=x&debug=true
    [WebMethod]
    public void Fetch(string branch, string persona, string func, int dpi = 600, int fetch = 50, string filter = null, bool debug = false)
    {
        HttpResponse Response = Context.Response;
        NameValueCollection settings = WebConfigurationManager.AppSettings;
        string platformId = Config.personaId.ToString();
        int xi_thread = 1;
        int hit_count = 0;
        bool trace = debug ;

        writeResponse(Response, String.Format("# {0:F}", DateTime.Now));
        writeResponse(Response, String.Format("# branch: {0}", branch));
        if (null == filter || filter.Length == 0)
        {
            filter = "/m/tcases/futures/next/wip/";
            writeResponse(Response, String.Format("# location: {0}", filter));
        }

        if (null != persona && persona.Length > 0)
        {            
            writeResponse(Response, String.Format("# persona: {0}", persona));
        }

        writeResponse(Response, String.Format("# resolution: {0}", dpi));
        writeResponse(Response, String.Format("# xi threads: {0}", xi_thread));

        try
        {
            DbConn.NewConnection(Config.getConnectionString());
            platformId = DbConn.ExecuteScalar(settings["platform"], branch, persona.Replace("64", ""), dpi).ToString();
            writeResponse(Response, String.Format("# platformId: {0}", platformId));
            string sql = String.Format(settings["query"], branch, filter, platformId, generateQuerySQL(func, platformId));
            if (-1 < fetch)
            {
                sql = String.Format("\r\nSELECT * FROM ({0}) WHERE ROWNO > {1} AND ROWNO <= ({1} + {2})", sql, 0, fetch);
            }

            DataTable tbl = DbConn.Query(sql);
            if (null != tbl)
            {
                hit_count = tbl.Rows.Count;
                writeResponse(Response, String.Format("# testcases: {0}", hit_count));
                foreach (DataRow row in tbl.Rows)
                {
                    string checksum = row["CHECKSUMS"].ToString().Trim();
                    string tcase = String.Format("{0} : {1}", row["TLOC"], checksum);
                    writeResponse(Response, tcase.Replace(filter, ""));
                }

                if (trace)
                {
                    writeResponse(Response, sql);
                }
            }
        }
        catch (EmptyResultException err)
        {
            writeResponse(Response, String.Format("# testcases: {0}", hit_count));
            if (trace)
            {
                writeResponse(Response, FlattenException(err));
            }
        }
        catch (Exception err)
        {
            writeResponse(Response, "# Exception occured");
            if (trace)
            {
                writeResponse(Response, String.Format("# Stacktrace: {0}", FlattenException(err)));
            }
        }
        finally
        {
            DbConn.Terminate();
        }

        //sampleText.Text = _testcases;
        
        Response.Flush();
        Response.End();
    }

    public static string FlattenException(Exception exception)
    {
        var stringBuilder = new StringBuilder();

        while (exception != null)
        {
            stringBuilder.AppendLine(exception.Message);
            stringBuilder.AppendLine(exception.StackTrace);

            exception = exception.InnerException;
        }

        return stringBuilder.ToString();
    }
}