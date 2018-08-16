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

    public bool read_checksums(string filename)
    {
        string line;
        string location = string.Empty;
        int PID = 4;
        int resolution = -1;
        string persona = "sim-color";

        StreamReader stream = new StreamReader(filename);
        try
        {
            do
            {
                line = stream.ReadLine().Trim();
                // # location : /m/tcases/futures/next/wip/
                MatchCollection matches = Regex.Matches(line, "# location : (?<location>.*)$");
                foreach (Match match in matches)
                {
                    location = match.Groups["location"].Value;
                    break;
                }

                matches = Regex.Matches(line, "# persona : (?<persona>.*)$");
                foreach (Match match in matches)
                {
                    persona = match.Groups["persona"].Value;
                    break;
                }

                matches = Regex.Matches(line, "# resolution : (?<resolution>.*)$");
                if (1 == matches.Count)
                {
                    resolution = Int32.Parse(matches[0].Groups["resolution"].Value);
                    break;
                }

            } while (line == string.Empty || line.StartsWith("#"));

            PLATFORM platform_table = new PLATFORM();
            PID = platform_table.lookup_pid(persona, resolution);

            return update_checksums(PID, location, stream);
        }
        finally
        {
            stream.Close();
        }
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
    public bool update_checksums(int PID, string location, StreamReader stream)
    {
        string line;
        string table_name = this.TableName;

        try
        {
            this.TableName = "TESTCASE_CHECKSUMS";

            do
            {
                line = stream.ReadLine();
                if (line != null)
                {
                    line = line.Trim();
                    if (line.StartsWith("#"))
                    {
                        Console.WriteLine("{0}\n", line);
                        continue;
                    }

                    String[] splitString = Regex.Split(line, @"\s*:\s*");
                    if (2 == splitString.Length)
                    {
                        string testcase = location + splitString[0];
                        string checksums = splitString[1];
                        string tguid = TESTCASE.lookup_tguid(testcase);
                        
                        TESTCASE_CHECKSUMS_VIEW.Row rec = NewRow();
                        rec.TGUID = tguid;
                        // rec.TNAME = match.Index + 1;
                        rec.PID = PID;
                        rec.CHECKSUMS = checksums;

                        merge(rec);
                    }
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
    DataColumn CHECKSUMS;   // CLOB
    #endregion
}
