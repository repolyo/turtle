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
public class TESTCASE_CHECKSUM : AbstractOracleDBTable<TESTCASE_CHECKSUM.Row>
{
    protected override void InitVars()
    {
        this.TGUID = AddColumn("TGUID", typeof(string));
        this.PAGE_NO = AddColumn("PAGE_NO", typeof(int));
        this.PID = AddColumn("PID", typeof(int));
        this.CHECKSUM = AddColumn("CHECKSUM", typeof(string));
    }
    
    public override string[] filters()
    {
        return new string[] { "TGUID",  "PAGE_NO", "PID" };
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

    public bool read_checksums(string filename)
    {
        string line;
        string location = string.Empty;
        int PID = 4;
        int resolution = -1;
        string persona = "sim-color";
        string branch = string.Empty;

        StreamReader stream = new StreamReader(filename);
        try
        {
            do
            {
                MatchCollection matches;
                line = stream.ReadLine().Trim();

                if (branch == string.Empty)
                {
                    // # branch : firmware-6
                    matches = Regex.Matches(line, "#\\s*branch\\s*:\\s*(?<branch>.*)$", RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                    {
                        branch = match.Groups["branch"].Value;
                        break;
                    }
                }

                if (location == string.Empty)
                {
                    // # location : /m/tcases/futures/next/wip/
                    matches = Regex.Matches(line, "#\\s*location\\s*:\\s*(?<location>.*)$", RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                    {
                        location = match.Groups["location"].Value;
                        break;
                    }
                }

                if (persona == string.Empty)
                {
                    matches = Regex.Matches(line, "#\\s*persona\\s*:\\s*(?<persona>.*)$", RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                    {
                        persona = match.Groups["persona"].Value;
                        break;
                    }
                }

                if (-1 == resolution)
                {
                    matches = Regex.Matches(line, "#\\s*resolution\\s*:\\s*(?<resolution>.*)$", RegexOptions.IgnoreCase);
                    if (1 == matches.Count)
                    {
                        resolution = Int32.Parse(matches[0].Groups["resolution"].Value);
                        break;
                    }
                }
            }
            while (line == string.Empty || line.StartsWith("#"));

            PLATFORM platform_table = new PLATFORM();
            PID = platform_table.lookup_pid (branch, persona, resolution);

            return update_checksums(PID, location, stream);
        }
        finally
        {
            stream.Close();
        }
    }

    /// <summary>
    /// pdf/font_mapping/test-embedded.pdf : bc31582e,f892ac0d,
    /// MERGE INTO TESTCASE_CHECKSUM using dual on (TGUID=? AND PID=? AND PAGE_NO=?)
    /// WHEN NOT matched THEN INSERT (TGUID,PID,PAGE_NO,CHECKSUM) VALUES (?,?,?,?)
    /// WHEN matched then UPDATE SET CHECKSUM=?"
    /// </summary>
    /// <param name="PID"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    public bool update_checksums(int PID, string location, StreamReader stream)
    {
        string pattern = @"([a-z\d]+)";
        string line;

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

                    MatchCollection matches = Regex.Matches(checksums, pattern);
                    foreach (Match match in matches)
                    {
                        TESTCASE_CHECKSUM.Row rec = NewRow();
                        rec.TGUID = tguid;
                        rec.PAGE_NO = match.Index + 1;
                        rec.PID = PID;
                        rec.CHECKSUM = match.Value;

                        merge(rec);
                    }
                }
            }
        } while (line != null); 

        return true;
    }

    //public int batchUpdate(String fmt, params Object[] args)
    //{
    //    int updated = 0;
    //    string sql = String.Format(fmt, args);
    //    try
    //    {
    //        OracleCommand cmd = new OracleCommand(sql, connection);

    //        string[] job_id_vals = new string[3] { "IT_DBA", "IT_MAN", "IT_VP" };

    //        OracleParameter p_job_id = new OracleParameter();
    //        p_job_id.OracleDbType =
    //        OracleDbType.Varchar2;
    //        p_job_id.Value = job_id_vals;

    //        cmd.ArrayBindCount = job_id_vals.Length;
    //        cmd.Parameters.Add(p_job_id);

    //        cmd.ExecuteNonQuery();
    //    }
    //    catch (Exception e)
    //    {
    //        throw new DBException(e, sql, args);
    //    }
    //    return updated;
    //}

    public class Row : AbstractDataRow
    {
        private TESTCASE_CHECKSUM table;
        internal Row(DataRowBuilder rb) : base(rb)
        {
            this.table = ((TESTCASE_CHECKSUM)(this.Table));
        }

        #region Properties
        public string TGUID
        {
            get { return ToString (table.TGUID); }
            set { this[table.TGUID] = value; }
        }
        public int PAGE_NO
        {
            get { return ToInteger(table.PAGE_NO); }
            set { this[table.PAGE_NO] = value; }
        }
        public int PID
        {
            get { return ToInteger(table.PID); }
            set { this[table.PID] = value; }
        }
        public string CHECKSUM
        {
            get { return ToString(table.CHECKSUM); }
            set { this[table.CHECKSUM] = value; }
        }
        #endregion
    }

    #region Class Members
    DataColumn TGUID;       // TGUID	VARCHAR2(32 BYTE)
    DataColumn PAGE_NO;     // PAGE_NO	NUMBER
    DataColumn PID;         // PID	NUMBER
    DataColumn CHECKSUM;    // VARCHAR2(10 BYTE)
    #endregion
}
