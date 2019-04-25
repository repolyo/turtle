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
    private int seqNo = 0;

    protected override void InitVars()
    {
        this.TGUID = AddColumn("TGUID", typeof(string));
        this.SEQ = AddColumn("SEQ", typeof(int));
        this.FID = AddColumn("FID", typeof(int));
    }

    public override string[] filters()
    {
        return new string[] { "TGUID", "FID" };
    }

    public new Row NewRow()
    {
        Row newRow = ((Row)(base.NewRow()));
        return newRow;
    }

    protected override string mergeUpdateValues(TableColumns cols)
    {
        return string.Format("{0}",
            cols.FormattedColumnValuePair("SEQ"));
    }

    public bool update (string tguid, FUNC.Row func)
    {
        TESTCASE_FUNC.Row tfunc = NewRow();
        tfunc.TGUID = tguid;
        tfunc.FID = func.FID;
        tfunc.SEQ = seqNo++;
        merge(tfunc);
        return true;
    }

    protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
    {
        return new Row(builder);
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
            get { return ToInteger(table.SEQ); }
            set { this[table.SEQ] = value; }
        }
        public Nullable<int> FID
        {
            get { return ToInteger(table.FID); }
            set { this[table.FID] = value; }
        }
        #endregion
    }

    #region Class Members
    DataColumn TGUID;
    DataColumn SEQ;
    DataColumn FID;
    #endregion
}