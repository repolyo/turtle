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

        StreamReader stream = new StreamReader(filename);
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
            line = stream.ReadLine().Trim();
            if (line != null)
            {
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
                        try
                        {
                            TESTCASE_CHECKSUM.Row rec = NewRow();
                            rec.TGUID = tguid;
                            rec.PAGE_NO = match.Index + 1;
                            rec.PID = PID;
                            rec.CHECKSUM = match.Value;

                            merge(rec);

                            Console.WriteLine("Match: {0} at index [{1}, {2})",
                                match.Value,
                                match.Index,
                                match.Index + match.Length);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: {0}\n", ex.ToString ());
                        }
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

    public class Row : DataRow
    {
        private TESTCASE_CHECKSUM table;
        internal Row(DataRowBuilder rb) : base(rb)
        {
            this.table = ((TESTCASE_CHECKSUM)(this.Table));
        }

        #region Properties
        public string TGUID
        {
            get { return this[table.TGUID].ToString (); }
            set { this[table.TGUID] = value; }
        }
        public int PAGE_NO
        {
            get { return Int32.Parse (this[table.PAGE_NO].ToString()); }
            set { this[table.PAGE_NO] = value; }
        }
        public int PID
        {
            get { return Int32.Parse (this[table.PID].ToString()); }
            set { this[table.PID] = value; }
        }
        public string CHECKSUM
        {
            get { return this[table.CHECKSUM].ToString (); }
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

    //protected string TGUID;
    //protected int PAGE_NO;
    //protected string CHECKSUM;
    //protected int PID;
    //OracleConnection connection;