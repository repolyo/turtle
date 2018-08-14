using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Tlib.Dao;
using TLib.Interfaces;
using TLib;

/// <summary>
/// Summary description for TESTCASE_FUNC
/// TGUID	VARCHAR2(32 BYTE)
/// SEQ	NUMBER
/// FID	NUMBER(38,0)
/// PID	NUMBER(38,0)
/// </summary>
public class TESTCASE_FUNC : AbstractOracleDBTable<TESTCASE_FUNC.Row>
{
    protected override void InitVars()
    {
        this.TGUID = AddColumn("TGUID", typeof(string));
        this.SEQ = AddColumn("SEQ", typeof(int));
        this.FID = AddColumn("FID", typeof(int));
        this.PID = AddColumn("PID", typeof(int));
    }

    public override string[] filters()
    {
        return new string[] { "TGUID", "FID", "PID" };
    }

    public new Row NewRow()
    {
        Row newRow = ((Row)(base.NewRow()));
        return newRow;
    }

    public class Row : AbstractDataRow
    {
        private TESTCASE_FUNC table;
        internal Row(DataRowBuilder rb)
            : base(rb)
        {
            this.table = ((TESTCASE_FUNC)(this.Table));
        }

        #region Properties
        public string TGUID
        {
            get { return this[table.TGUID].ToString(); }
            set { this[table.TGUID] = value; }
        }
        public int SEQ
        {
            get { return Int32.Parse (this[table.SEQ].ToString()); }
            set { this[table.SEQ] = value; }
        }
        public int FID
        {
            get { return Int32.Parse (this[table.FID].ToString()); }
            set { this[table.FID] = value; }
        }
        public int PID
        {
            get { return Int32.Parse (this[table.PID].ToString()); }
            set { this[table.PID] = value; }
        }
        #endregion
    }

    #region Class Members
    DataColumn TGUID;
    DataColumn SEQ;
    DataColumn FID;
    DataColumn PID;
    #endregion
}