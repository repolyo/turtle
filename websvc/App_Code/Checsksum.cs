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
        NameValueCollection settings = WebConfigurationManager.AppSettings;
        string funcList = (null == filter) ? "%" : filter.Trim();
        funcList = funcList.Replace('*', '%').Replace('?', '_');

        StringBuilder testcaseByFunc = new StringBuilder();
        string[] funcs = funcList.Split(',');
        foreach (string func in funcs)
        {
            if (func == "main") continue;
            testcaseByFunc.Append(String.Format("UPPER('{0}'), ", func));
        }
        
        return String.Format(settings["query"], pid, testcaseByFunc);
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

    // sample usage:
    //  http://localhost/testcases/Checksum.asmx/Get?persona=sim-atlantis&func=pdflex_add_object&dpi=600&fetch=10&filter=x&debug=true
    [WebMethod]
    public void Get(string persona, string func, int dpi = 600, int fetch = 50, string filter = null, bool debug = false)
    {
        HttpResponse Response = Context.Response;
        string platformId = Config.personaId.ToString();
        int xi_thread = 1;
        int hit_count = 0;
        writeResponse(Response, String.Format("# {0:F}", DateTime.Now));
        if (null == filter || filter.Length == 0)
        {
            filter = "/m/tcases/futures/next/wip/";
            writeResponse(Response, String.Format("# location: {0}", filter));
        }

        if (null != persona && persona.Length > 0)
        {
            platformId = String.Format("(select PID from PLATFORM where PERSONA='{0}' AND RESOLUTION={1})",
                persona.Replace("64", ""), // sim-color and sim64-color checksums are just the same.
                dpi, xi_thread);
            writeResponse(Response, String.Format("# persona: {0}", persona));
        }

        string sql = "\r\nSELECT ROW_NUMBER() OVER (ORDER BY a.TLOC ASC) AS ROWNO, " +
                     "  a.TLOC, " +
                     "  cs.CHECKSUMS " +
                     "FROM TESTCASE a join TESTCASE_CHECKSUMS cs on a.tguid = cs.tguid " +
                     "WHERE a.HIDDEN <> 'Y' AND a.TLOC LIKE '%" + filter + "%' AND a.TGUID IN (" + generateQuerySQL (func, platformId) + ")";

        if ( -1 < fetch ) {
            sql = String.Format("\r\nSELECT * FROM ({0}) WHERE ROWNO > {1} AND ROWNO <= ({1} + {2})", sql, 0, fetch);
        }

        writeResponse(Response, String.Format("# resolution: {0}", dpi));
        writeResponse(Response, String.Format("# xi threads: {0}", xi_thread));

        try
        {
            DbConn.NewConnection(Config.getConnectionString());
            DataTable tbl = DbConn.Query(sql);
            if (null != tbl)
            {
                hit_count = tbl.Rows.Count;
                writeResponse(Response, String.Format("# testcases: {0}", hit_count));
                foreach (DataRow row in tbl.Rows)
                {
                    string checksum = row["CHECKSUMS"].ToString().Trim();
                    if (checksum.Length == 0) continue;
                    string tcase = String.Format("{0} : {1}", row["TLOC"], checksum);
                    writeResponse(Response, tcase.Replace(filter, ""));
                }

                if (debug)
                {
                    writeResponse(Response, sql);
                }
            }
        }
        catch (EmptyResultException err)
        {
            writeResponse(Response, String.Format("# testcases: {0}", hit_count));
            if (debug)
            {
                writeResponse(Response, FlattenException(err));
            }
        }
        catch (Exception err)
        {
            writeResponse(Response, "# Exception occured");
            if (debug)
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