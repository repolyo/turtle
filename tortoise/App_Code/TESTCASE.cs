using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Tlib.Dao;
using TLib.Interfaces;
using TLib;

/// <summary>
/// Summary description for TESTCASE
/// </summary>
public class TESTCASE : AbstractOracleDBTable<TESTCASE.Row>
{
    private List<string> sort_columns;

    public TESTCASE ()
    {
        sort_columns = new List<string>();
    }

    protected override void InitVars()
    {
        this.TGUID = AddColumn("TGUID", typeof(string)); // TGUID	VARCHAR2(32 BYTE)
        this.TNAME = AddColumn("TNAME", typeof(string)); // TNAME	VARCHAR2(255 BYTE)
        this.TLOC = AddColumn("TLOC", typeof(string)); // TLOC	VARCHAR2(255 BYTE)
        this.TTYPE = AddColumn("TTYPE", typeof(string)); // TTYPE	VARCHAR2(20 BYTE)
        this.TSIZE = AddColumn("TSIZE", typeof(int)); // TSIZE	NUMBER
        this.HIDDEN = AddColumn("HIDDEN", typeof(char)); // HIDDEN	CHAR(1 BYTE)
        // UPDATE_DATE	TIMESTAMP(6)
    }

    public override string[] filters()
    {
        return new string[] { "TLOC" };
    }

    protected override string[] sort()
    {
        return sort_columns.ToArray();
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

    public static string lookup_tguid(string testcase)
    {
        TESTCASE tbl = new TESTCASE();
        Row row = tbl.NewRow();
        row.TLOC = testcase;

        row = tbl.findSingleResult(row);
        if (null != row)
        {
            return row.TGUID;
        }
        return string.Empty;
    }

    public DataTable QueryTestcases(string Filter, string sortColumns, int startRecord, int maxRecords)
    {
        Row filter = this.NewRow();
        filter.HIDDEN = 'N';
        filter.TNAME = Filter;
        filter.Range = new Range<int>(startRecord, 0, maxRecords);

        sort_columns.Clear();
        sort_columns.Add(sortColumns);

        return findAll(filter);
    }

    public int SelectCount(string Filter)
    {
        Row filter = this.NewRow();
        filter.HIDDEN = 'N';
        filter.TNAME = Filter;

        return (int)base.peekResultCount(filter);
    }

    /// <summary>
    /// a.TGUID IN (SELECT UNIQUE t.TGUID FROM TESTCASE_FUNC t , FUNC f WHERE t.fid = f.fid AND f.FUNC_NAME LIKE '{0}'))
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    protected override string buildWhereSQL(TESTCASE.Row filter, List<DataColumn> where = null)
    {
        //string whereSQL = string.Empty;
        //if (null != where && where.Count > 0)
        //{
        //    foreach (DataColumn field in where)
        //    {
        //        object value = getFieldValue(field, filter);
        //        if (null != value)
        //        {
        //            whereSQL += string.Format("{0},", TableColumns.Format(field, value));
        //        }
        //    }
        //}

        return string.Format("TGUID IN (SELECT UNIQUE t.TGUID FROM TESTCASE_FUNC t, FUNC f WHERE t.fid = f.fid AND f.FUNC_NAME LIKE '{0}')", filter.TNAME);
    }

    protected override string buildSelectSQL(TESTCASE.Row filter, StringArraySQL cols = null, List<DataColumn> where = null, List<DataColumn> sorting = null)
    {
        string sql = string.Empty;
        Range<int> range = filter.Range;
        if (null != range)
        {
            sql = string.Format("SELECT * FROM ({0}) WHERE ROWNO > {1} AND ROWNO <= ({1} + {2})",
                            base.buildSelectSQL(filter, cols, where, sorting),
                            range.Min, range.Max);
        }
        else {
            sql = base.buildSelectSQL(filter, cols, where, sorting);
        }

        return sql;
    }

    public class Row : AbstractDataRow
    {
        private TESTCASE table;

        internal Row(DataRowBuilder rb)
            : base(rb)
        {
            this.table = ((TESTCASE)(this.Table));
        }

        public override List<string> Columns()
        {
            AbstractTableDB<Row>.StringArraySQL cols = new AbstractTableDB<Row>.StringArraySQL();

            cols.Add("ROW_NUMBER() OVER(ORDER BY UPDATE_DATE DESC NULLS LAST) AS ROWNO");
            
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
        public string TNAME
        {
            get { return ToString(table.TNAME); }
            set { this[table.TNAME] = value; }
        }
        public string TLOC
        {
            get { return ToString(table.TLOC); }
            set { this[table.TLOC] = value; }
        }
        public string TTYPE
        {
            get { return ToString(table.TTYPE); }
            set { this[table.TTYPE] = value; }
        }
        public int TSIZE
        {
            get { return ToInteger(table.TSIZE); }
            set { this[table.TSIZE] = value; }
        }
        public char HIDDEN
        {
            get { return (char)(IsNull(table.HIDDEN) ? null : this[table.HIDDEN]); }
            set { this[table.HIDDEN] = value; }
        }
        #endregion
    }

    #region Class Members
    DataColumn TGUID;       // TGUID	VARCHAR2(32 BYTE)
    DataColumn TNAME;
    DataColumn TLOC;
    DataColumn TTYPE;
    DataColumn TSIZE;
    DataColumn HIDDEN;
    #endregion
}
