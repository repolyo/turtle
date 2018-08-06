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
    //private static readonly OraclConnectionPool.OracleDBConn connection 
    //    = OraclConnectionPool.GetInstance().checkOut(new string[] { Config.getConnectionString() });

    internal virtual void InitVars()
    {
        this.tguid = AddColumn("tguid", typeof(string));
        this.page_no = AddColumn("page_no", typeof(int));
        this.checksum = AddColumn("checksum", typeof(string));
    }
    
    public override string[] filters()
    {
        return new string[] { "tguid", "page_no" };
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
            return update_checksums(4, new StreamReader(filename));
    }

    /// <summary>
    /// pdf/font_mapping/test-embedded.pdf : bc31582e,f892ac0d,
    /// </summary>
    /// <param name="pid"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    public bool update_checksums(int pid, StreamReader stream)
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
                    string testcase = splitString[0];
                    string checksums = splitString[1];

                    MatchCollection matches = Regex.Matches(checksums, pattern);
                    foreach (Match match in matches)
                    {
                        Console.WriteLine("Match: {0} at index [{1}, {2})",
                            match.Value,
                            match.Index,
                            match.Index + match.Length);
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
        public string tguid
        {
            get { return this[table.tguid].ToString (); }
            set { this[table.tguid] = value; }
        }
        public int page_no
        {
            get { return (int)this[table.page_no]; }
            set { this[table.page_no] = value; }
        }
        public string checksum
        {
            get { return this[table.checksum].ToString (); }
            set { this[table.checksum] = value; }
        }
        #endregion
    }

    #region Class Members
    DataColumn tguid;       // TGUID	VARCHAR2(32 BYTE)
    DataColumn page_no;     // PAGE_NO	NUMBER
    DataColumn checksum;    // VARCHAR2(10 BYTE)
    #endregion
}

    //protected string tguid;
    //protected int page_no;
    //protected string checksum;
    //protected int pid;
    //OracleConnection connection;