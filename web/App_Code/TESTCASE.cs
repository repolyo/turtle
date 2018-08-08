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
    protected override void InitVars()
    {
        this.TGUID = AddColumn("TGUID", typeof(string)); // TGUID	VARCHAR2(32 BYTE)
        this.TNAME = AddColumn("TNAME", typeof(string)); // TNAME	VARCHAR2(255 BYTE)
        this.TLOC = AddColumn("TLOC", typeof(string)); // TLOC	VARCHAR2(255 BYTE)
        this.TTYPE = AddColumn("TTYPE", typeof(string)); // TTYPE	VARCHAR2(20 BYTE)
        this.TSIZE = AddColumn("TSIZE", typeof(int)); // TSIZE	NUMBER
        this.HIDDEN = AddColumn("HIDDEN", typeof(bool)); // HIDDEN	CHAR(1 BYTE)
// UPDATE_DATE	TIMESTAMP(6)
    }

    public override string[] filters()
    {
        return new string[] { "TLOC" };
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

    public class Row : AbstractDataRow
    {
        private TESTCASE table;
        internal Row(DataRowBuilder rb)
            : base(rb)
        {
            this.table = ((TESTCASE)(this.Table));
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
        public bool HIDDEN
        {
            get { return ToBoolean(table.HIDDEN); }
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
