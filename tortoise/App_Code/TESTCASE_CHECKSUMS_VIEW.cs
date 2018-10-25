using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Tlib.Dao;
using TLib.Interfaces;
using TLib;
using System.Text.RegularExpressions;
using System.IO;

/// <summary>
/// Summary description for TestcaseChecksum
/// TGUID	VARCHAR2(32 BYTE)
/// PAGE_NO	NUMBER
/// CHECKSUM	VARCHAR2(10 BYTE)
/// PID	NUMBER
/// </summary>
public class TESTCASE_CHECKSUMS_VIEW : AbstractOracleDBTable<TESTCASE_CHECKSUMS_VIEW.Row>
{
    protected override void InitVars()
    {
        this.TGUID = AddColumn("TGUID", typeof(string));
        this.TLOC = AddColumn("TLOC", typeof(string));
        this.PID = AddColumn("PID", typeof(int));
        this.MODIFIED_BY = AddColumn("MODIFIED_BY", typeof(string));
        this.CHECKSUMS = AddColumn("CHECKSUMS", typeof(string));
    }

    public override string[] filters()
    {
        return new string[] { "TGUID", "PID" };
    }

    protected override Row GetFilter(string Filter)
    {
        Row filter = this.NewRow();

        if (!string.IsNullOrEmpty (Filter) ) {
            filter.PID = Int32.Parse(Filter);
        }

        return filter;
    }

    public new Row NewRow()
    {
        Row newRow = ((Row)(base.NewRow()));
        return newRow;
    }

    protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
    {
        return new Row(builder);
    }

    public string Truncate(string value, int maxChars)
    {
        return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
    }

    public override DataTable QueryTestcases(string Filter, string sortColumns, int startRecord, int maxRecords)
    {
        DataTable dt = base.QueryTestcases(Filter, sortColumns, startRecord, maxRecords);
        foreach (TESTCASE_CHECKSUMS_VIEW.Row r in dt.Rows)
        {
            r.TLOC = Truncate(r.TLOC, 80);
            r.CHECKSUMS = Truncate (r.CHECKSUMS, 130);
        }
        return dt;
    }

    public bool update_checksums(string user_id, string filename)
    {
        string line;
        string location = string.Empty;
        int PID = 4;
        int resolution = -1;
        string persona = string.Empty;
        string branch = string.Empty;

        StreamReader stream = new StreamReader(filename);
        try
        {
            do
            {
                MatchCollection matches;
                line = stream.ReadLine().Trim();

                if (branch == string.Empty) {
                    // # branch : firmware-6
                    matches = Regex.Matches(line, "#\\s*branch\\s*:\\s*(?<branch>.*)$", RegexOptions.IgnoreCase);
                    foreach (Match match in matches) {
                        branch = match.Groups["branch"].Value;
                        break;
                    }
                }

                if (location == string.Empty) {
                    // # location : /m/tcases/futures/next/wip/
                    matches = Regex.Matches(line, "#\\s*location\\s*:\\s*(?<location>.*)$", RegexOptions.IgnoreCase);
                    foreach (Match match in matches) {
                        location = match.Groups["location"].Value;
                        break;
                    }
                }

                if (persona == string.Empty) {
                    matches = Regex.Matches(line, "#\\s*persona\\s*:\\s*(?<persona>.*)$", RegexOptions.IgnoreCase);
                    foreach (Match match in matches) {
                        persona = match.Groups["persona"].Value;
                        break;
                    }
                }

                if (-1 == resolution) {
                    matches = Regex.Matches(line, "#\\s*resolution\\s*:\\s*(?<resolution>.*)$", RegexOptions.IgnoreCase);
                    if (1 == matches.Count) {
                        resolution = Int32.Parse(matches[0].Groups["resolution"].Value);
                        break;
                    }
                }
            }
            while (line == string.Empty || line.StartsWith("#"));

            PLATFORM platform_table = new PLATFORM();
            PID = platform_table.lookup_pid (branch, persona, resolution);

            return update_checksums(user_id, PID, location, stream);
        }
        finally
        {
            stream.Close();
        }
    }

    protected override string mergeUpdateValues(TableColumns cols)
    {
        return string.Format ("{0}, {1}", cols.FormattedColumnValuePair("MODIFIED_BY"), cols.FormattedColumnValuePair("CHECKSUMS"));
    }

    /// <summary>
    /// pdf/font_mapping/test-embedded.pdf : bc31582e,f892ac0d,
    /// MERGE INTO TESTCASE_CHECKSUMS_VIEW using dual on (TGUID=? AND PID=? AND PAGE_NO=?)
    /// WHEN NOT matched THEN INSERT (TGUID,PID,PAGE_NO,CHECKSUM) VALUES (?,?,?,?)
    /// WHEN matched then UPDATE SET CHECKSUM=?"
    /// </summary>
    /// <param name="PID"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    public bool update_checksums(string user_id, int PID, string location, StreamReader stream)
    {
        string line;
        string table_name = this.TableName;
        string testcase = string.Empty;
        try
        {
            this.TableName = "TESTCASE_CHECKSUMS";

            do
            {
                line = stream.ReadLine();
                if (line == null) continue;

                try
                {
                    line = line.Trim();
                    if (line.StartsWith("#"))
                    {
                        Console.WriteLine("{0}\n", line);
                        continue;
                    }

                    String[] splitString = Regex.Split(line, @"\s*[: ]\s*");
                    if (2 == splitString.Length)
                    {
                        testcase = location + splitString[0];
                        string checksums = splitString[1];
                        string tguid = TESTCASE.lookup_tguid(testcase);
                        
                        TESTCASE_CHECKSUMS_VIEW.Row rec = NewRow();
                        rec.TGUID = tguid;

                        // rec.TNAME = match.Index + 1;
                        rec.MODIFIED_BY = user_id;
                        rec.PID = PID;
                        rec.CHECKSUMS = checksums;

                        merge(rec);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(testcase + " ERROR: " + e.StackTrace);
                }
            } while (line != null);
        }
        finally
        {
            this.TableName = table_name;
        }

        return true;
    }

    public class Row : AbstractDataRow
    {
        private TESTCASE_CHECKSUMS_VIEW table;
        internal Row(DataRowBuilder rb) : base(rb)
        {
            this.table = ((TESTCASE_CHECKSUMS_VIEW)(this.Table));
        }

        public override List<string> Columns()
        {
            AbstractTableDB<Row>.StringArraySQL cols = new AbstractTableDB<Row>.StringArraySQL();

            cols.Add("ROW_NUMBER() OVER(ORDER BY TLOC ASC) AS ROWNO");

            foreach (DataColumn dc in base.Table.Columns)
            {
                cols.Add(dc.ColumnName);
            }

            return cols;
        }

        #region Properties
        public string TGUID
        {
            get { return ToString(table.TGUID); }
            set { this[table.TGUID] = value; }
        }
        public string TLOC
        {
            get { return ToString(table.TLOC); }
            set { this[table.TLOC] = value; }
        }
        public int PID
        {
            get { return ToInteger(table.PID); }
            set { this[table.PID] = value; }
        }
        public string MODIFIED_BY
        {
            get { return ToString(table.MODIFIED_BY); }
            set { this[table.MODIFIED_BY] = value; }
        }
        public string CHECKSUMS
        {
            get { return ToString(table.CHECKSUMS); }
            set { this[table.CHECKSUMS] = value; }
        }
        #endregion
    }

    #region Class Members
    DataColumn TGUID;       // TGUID	VARCHAR2(32 BYTE)
    DataColumn TLOC;        // TLOC	VARCHAR2(255)
    DataColumn PID;         // PID	NUMBER
    DataColumn MODIFIED_BY;
    DataColumn CHECKSUMS;   // CLOB
    #endregion
}
