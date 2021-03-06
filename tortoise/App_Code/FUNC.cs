﻿using System;
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
/// Summary description for FUNC
/// 
/// FID	NUMBER(38,0)
/// SOURCE_FILE	VARCHAR2(255 BYTE)
/// LINE_NO	NUMBER(38,0)
/// FUNC_NAME	VARCHAR2(255 BYTE)
/// </summary>
public class FUNC : AbstractOracleDBTable<FUNC.Row>
{
    protected override void InitVars()
    {
        this.FID = AddColumn("FID", typeof(int));
        this.DEPRECATED = AddColumn("DEPRECATED", typeof(int));
        this.SOURCE_FILE = AddColumn("SOURCE_FILE", typeof(string));
        this.LINE_NO = AddColumn("LINE_NO", typeof(int));
        this.FUNC_NAME = AddColumn("FUNC_NAME", typeof(string));
    }

    public override string[] filters()
    {
        return new string[] { "FID" };
    }

    public new Row NewRow()
    {
        FUNC.Row newRow = ((FUNC.Row)(base.NewRow()));
        newRow.DEPRECATED = 0;
        return newRow;
    }

    protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
    {
        return new Row(builder);
    }


    public Nullable<int> lookup_fid(Row rec)
    {
        try {
            object value = ExecuteScalar(String.Format ("SELECT GENERATE_FID ('{0}', '{1}') FROM DUAL", rec.SOURCE_FILE, rec.FUNC_NAME) ); 
            return (int)Converter.Format(value, TypeCode.Int32);
        }
        catch (Exception e)
        {
            Console.WriteLine(" ERROR: " + e.StackTrace);
            return 0;
        }
    }

    protected override string buildWhereSQL(Row filter, List<DataColumn> where = null)
    {
        string whereSQL = "DEPRECATED = 0"; // base.buildWhereSQL (filter, where);
        string file = (string)getFieldValue("SOURCE_FILE", filter);
        string func = (string)getFieldValue("FUNC_NAME", filter);

        if (null != file && string.Empty != file && null != func && string.Empty != func)
        {
            whereSQL += string.Format("{} FID IN (SELECT GENERATE_FID ('{0}',  '{1}') FROM DUAL) ",
                (whereSQL.Length > 0) ? "AND" : "", file, func);
        }
        //throw new Exception(whereSQL);
        return whereSQL;
    }
    

    protected override string mergeUpdateValues(TableColumns cols)
    {
        return string.Format("{0}, {1}, {2}",
            cols.FormattedColumnValuePair("SOURCE_FILE"),
            cols.FormattedColumnValuePair("FUNC_NAME"),
            cols.FormattedColumnValuePair("DEPRECATED"));
    }

    public bool update_func_mappings (string user_id, string filename)
    {
        string testcase = string.Empty;
        string line = string.Empty;

        StreamReader stream = new StreamReader(filename);
        try
        {
            TESTCASE_FUNC test_func = new TESTCASE_FUNC ();
            TESTCASE tcases = new TESTCASE();
            while (null != (line = stream.ReadLine()))
            {
                MatchCollection matches;
                if (string.Empty == testcase)
                {
                    // "Profiled target:  ./pdls -s -e pdf /m/tcases/futures/next/wip/pdf/fonts/report.pdf (PID 23196, part 1)"
                    matches = Regex.Matches(line, "Profiled target:.*-e\\s*(?<emul>[^\\s]+)\\s*(?<testcase>[^\\s]+)", RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                    {
                        TESTCASE.Row t = tcases.NewRow();
                        t.TLOC = match.Groups["testcase"].Value;
                        t.TTYPE =  match.Groups["emul"].Value.ToUpper();
                        t.TNAME = t.TLOC.Substring(t.TLOC.LastIndexOf('/') + 1);
                        t.HIDDEN = 'N';
                        tcases.merge(t);
                        t = tcases.lookup (t);
                        testcase = t.TGUID;
                    }
                }
                else
                {
                    // /usr/src/debug/graphen/0.0+gitAUTOINC+a8befc5ef3-r0/git/xi/fonts.c:AddName
                    matches = Regex.Matches(line, "\\s*(?<path>/.*)/(?<file>.*):(?<func>\\w+)", RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                    {
                        try
                        {
                            FUNC.Row func = NewRow();
                            string file = string.Format("{0}/{1}", match.Groups["path"].Value, match.Groups["file"].Value);
                            string name = match.Groups["func"].Value;

                            func.SOURCE_FILE = file;
                            func.FUNC_NAME = name;
                            func.LINE_NO = 0;
                            func.FID = lookup_fid(func);

                            Console.WriteLine("{0} ==> {1}", func.SOURCE_FILE, func.FUNC_NAME);

                            merge(func);

                            test_func.update(testcase, func);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(testcase + " ERROR: " + e.StackTrace);
                            throw e;
                        }
                        break;
                    }
                }
            }

            return true;
        }
        finally
        {
            stream.Close();
        }
    }

    public class Row : AbstractDataRow
    {
        private FUNC table;
        internal Row(DataRowBuilder rb)
            : base(rb)
        {
            this.table = ((FUNC)(this.Table));
        }

        protected Nullable<int> ToInt32(DataColumn col)
        {
            if (!this.IsNull(col))
            {
                return Int32.Parse(this[col].ToString());
            }
            return null;
        }

        public override List<string> Columns()
        {
            AbstractTableDB<Row>.StringArraySQL cols = new AbstractTableDB<Row>.StringArraySQL();

            cols.Add("ROW_NUMBER() OVER(ORDER BY FID ASC NULLS LAST) AS ROWNO");

            foreach (DataColumn dc in base.Table.Columns)
            {
                if ("NEW_ID" == dc.ColumnName || "ROWNO" == dc.ColumnName)
                {
                    continue;
                } 
                cols.Add(dc.ColumnName);
            }

            return cols;
        }

        #region Properties
        public string FUNC_NAME
        {
            get { return this[table.FUNC_NAME].ToString(); }
            set { this[table.FUNC_NAME] = value; }
        }
        public int LINE_NO
        {
            get { return ToInteger(table.LINE_NO); }
            set { this[table.LINE_NO] = value; }
        }
        public Nullable<int> FID
        {
            get { return ToInt32(table.FID); }
            set { this[table.FID] = value; }
        }
        public string SOURCE_FILE
        {
            get { return this[table.SOURCE_FILE].ToString(); }
            set { this[table.SOURCE_FILE] = value; }
        }
        public Nullable<int> DEPRECATED
        {
            get { return ToInt32(table.DEPRECATED); }
            set { this[table.DEPRECATED] = value; }
        }
        #endregion
    }

    #region Class Members
    DataColumn FUNC_NAME;
    DataColumn LINE_NO;
    DataColumn FID;
    DataColumn SOURCE_FILE;
    DataColumn DEPRECATED;
    #endregion
}