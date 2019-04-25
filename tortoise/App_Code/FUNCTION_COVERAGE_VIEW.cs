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
public class FUNCTION_COVERAGE_VIEW : AbstractOracleDBTable<FUNCTION_COVERAGE_VIEW.Row>
{
    protected override void InitVars()
    {
        //EMULATOR, TOTAL, USED, COVERAGE
        this.EMULATOR = AddColumn("EMULATOR", typeof(string));
        this.LCOV = AddColumn("LCOV", typeof(int));
        this.TOTAL = AddColumn("TOTAL", typeof(int));
        this.USED = AddColumn("USED", typeof(int));
        this.COVERAGE = AddColumn("COVERAGE", typeof(string));
    }

    public override string[] filters()
    {
        return new string[] { "EMULATOR" };
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

    public DataTable QueryFunctionCoverage(string Filter, string sortColumns, int startRecord, int maxRecords)
    {
        DataTable dt = base.QueryTestcases(Filter, sortColumns, startRecord, maxRecords);
        return dt;
    }
    
    public class Row : AbstractDataRow
    {
        private FUNCTION_COVERAGE_VIEW table;
        internal Row(DataRowBuilder rb) : base(rb)
        {
            this.table = ((FUNCTION_COVERAGE_VIEW)(this.Table));
        }

        public override List<string> Columns()
        {
            AbstractTableDB<Row>.StringArraySQL cols = new AbstractTableDB<Row>.StringArraySQL();
            foreach (DataColumn dc in base.Table.Columns)
            {
                cols.Add(dc.ColumnName);
            }

            return cols;
        }

        #region Properties
        
        public string EMULATOR
        {
            get { return ToString(table.EMULATOR); }
            set { this[table.EMULATOR] = value; }
        }

        public int LCOV
        {
            get { return ToInteger(table.LCOV); }
            set { this[table.LCOV] = value; }
        }

        public int TOTAL
        {
            get { return ToInteger(table.TOTAL); }
            set { this[table.TOTAL] = value; }
        }
        public int USED
        {
            get { return ToInteger(table.USED); }
            set { this[table.USED] = value; }
        }
        public string COVERAGE
        {
            get { return ToString(table.COVERAGE); }
            set { this[table.COVERAGE] = value; }
        }
        #endregion
    }

    #region Class Members
    DataColumn EMULATOR;     // VARCHAR2(32 BYTE)
    DataColumn TOTAL;        // NUMBER
    DataColumn LCOV;         // NUMBER
    DataColumn USED;         // NUMBER
    DataColumn COVERAGE;
    #endregion
}
