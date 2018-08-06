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
    internal virtual void InitVars()
    {
        this.tguid = AddColumn("tguid", typeof(string)); // TGUID	VARCHAR2(32 BYTE)
        this.name = AddColumn("tname", typeof(string)); // TNAME	VARCHAR2(255 BYTE)
        this.location = AddColumn("tloc", typeof(string)); // TLOC	VARCHAR2(255 BYTE)
        this.type = AddColumn("ttype", typeof(string)); // TTYPE	VARCHAR2(20 BYTE)
        this.size = AddColumn("tsize", typeof(int)); // TSIZE	NUMBER
        this.hidden = AddColumn("hidden", typeof(bool)); // HIDDEN	CHAR(1 BYTE)
// UPDATE_DATE	TIMESTAMP(6)
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

    public class Row : DataRow
    {
        private TESTCASE table;
        internal Row(DataRowBuilder rb)
            : base(rb)
        {
            this.table = ((TESTCASE)(this.Table));
        }

        #region Properties
        public string tguid
        {
            get { return this[table.tguid].ToString(); }
            set { this[table.tguid] = value; }
        }
        public int name
        {
            get { return (int)this[table.name]; }
            set { this[table.name] = value; }
        }
        public string location
        {
            get { return this[table.location].ToString(); }
            set { this[table.location] = value; }
        }
        public string type
        {
            get { return this[table.type].ToString(); }
            set { this[table.type] = value; }
        }
        public int size
        {
            get { return (int)this[table.size]; }
            set { this[table.size] = value; }
        }
        #endregion
    }

    #region Class Members
    DataColumn tguid;       // TGUID	VARCHAR2(32 BYTE)
    DataColumn name;
    DataColumn location;
    DataColumn type;
    DataColumn size;
    DataColumn hidden;
    #endregion
}
