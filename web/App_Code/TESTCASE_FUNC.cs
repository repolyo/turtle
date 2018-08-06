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
    internal virtual void InitVars()
    {
        this.tguid = AddColumn("TGUID", typeof(string));
        this.sequence_no = AddColumn("SEQ", typeof(int));
        this.function_id = AddColumn("FID", typeof(int));
        this.platform_id = AddColumn("PID", typeof(int));
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

    public class Row : DataRow
    {
        private TESTCASE_FUNC table;
        internal Row(DataRowBuilder rb)
            : base(rb)
        {
            this.table = ((TESTCASE_FUNC)(this.Table));
        }

        #region Properties
        public string tguid
        {
            get { return this[table.tguid].ToString(); }
            set { this[table.tguid] = value; }
        }
        public int sequence_no
        {
            get { return (int)this[table.sequence_no]; }
            set { this[table.sequence_no] = value; }
        }
        public int function_id
        {
            get { return (int)this[table.function_id]; }
            set { this[table.function_id] = value; }
        }
        public int platform_id
        {
            get { return (int)this[table.platform_id]; }
            set { this[table.platform_id] = value; }
        }
        #endregion
    }

    #region Class Members
    DataColumn tguid;
    DataColumn sequence_no;
    DataColumn function_id;
    DataColumn platform_id;
    #endregion
}