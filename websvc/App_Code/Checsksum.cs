using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using System.Text;
using System.Data;

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

        StringBuilder testcaseByFunc = new StringBuilder("SELECT cs.TGUID, listagg(cs.CHECKSUM, ',') " +
            " within GROUP (ORDER BY cs.PAGE_NO) AS CHECKSUM FROM TESTCASE_CHECKSUM cs WHERE " +
            " cs.pid = " + pid +
            " AND cs.TGUID IN (SELECT UNIQUE t.TGUID FROM TESTCASE_FUNC t, FUNC f WHERE t.fid = f.fid " +
            " AND UPPER(f.FUNC_NAME) IN ( ");

        string[] funcs = funcList.Split(',');
        foreach (string func in funcs)
        {
            if (func == "main") continue;
            testcaseByFunc.Append(String.Format("UPPER('{0}'), ", func));
        }
        testcaseByFunc.Append(" '__end__' ) ) GROUP BY cs.tguid");

        return testcaseByFunc.ToString();
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

        writeResponse(Response, String.Format("# {0:F}", DateTime.Now));
        if (null == filter || filter.Length == 0)
        {
            filter = "/m/tcases/futures/next/wip/";
            writeResponse(Response, String.Format("# location: {0}", filter));
        }

        if (null != persona && persona.Length > 0)
        {
            platformId = String.Format("(select PID from PLATFORM where PERSONA='{0}' AND RESOLUTION={1})", 
                persona, dpi, xi_thread);
            writeResponse(Response, String.Format("# persona: {0}", persona));
        }

        string sql = "SELECT ROW_NUMBER() OVER (ORDER BY a.UPDATE_DATE DESC) AS ROWNO, " +
                    "  a.TLOC, " +
                    "  b.CHECKSUM " +
                    "FROM TESTCASE a, " + "(" + generateQuerySQL(func, platformId) + ") b " +
                    "WHERE a.TGUID=b.TGUID AND a.HIDDEN <> 'Y' AND a.TLOC LIKE '%" + filter + "%'";

        if ( -1 < fetch ) {
            sql = String.Format("SELECT * FROM ({0}) WHERE ROWNO > {1} AND ROWNO <= ({1} + {2})", sql, 0, fetch);
        }

        writeResponse(Response, String.Format("# resolution: {0}", dpi));
        writeResponse(Response, String.Format("# xi threads: {0}", xi_thread));
        
        try
        {
            DbConn.NewConnection(Config.getConnectionString());
            DataTable tbl = DbConn.Query(sql);
            writeResponse(Response, String.Format("# testcases: {0}", tbl.Rows.Count));
            if (null != tbl) foreach (DataRow row in tbl.Rows)
                {
                    string checksum = row["CHECKSUM"].ToString().Trim();
                    if (checksum.Length == 0) continue;
                    string tcase = String.Format("{0} : {1}", row["TLOC"], checksum);
                    writeResponse(Response, tcase.Replace(filter, ""));
                }
        }
        catch (Exception err)
        {
            writeResponse(Response, String.Format("# {0}", err.GetBaseException().Message));
        }
        finally
        {
            DbConn.Terminate();
        }

        //sampleText.Text = _testcases;
        if (debug)
        {
            writeResponse(Response, "# -- SQL Query:  " + sql + ";\n\n\n");
        }

        Response.Flush();
        Response.End();
    }
}