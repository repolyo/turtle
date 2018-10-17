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
/// Summary description for PLATFORM
/// PID	NUMBER(38,0)
/// CODELINE	VARCHAR2(255 BYTE)
/// PERSONA	VARCHAR2(255 BYTE)
/// VERSION	VARCHAR2(255 BYTE)
/// MEMORY	NUMBER(32,0)
/// DISK	NUMBER(32,0)
/// USB	NUMBER(1,0)
/// RESOLUTION	NUMBER(38,0)
/// XITHREADCOUNT	NUMBER(38,0)
/// BUILD_NO	VARCHAR2(1024 BYTE)
/// </summary>
public class PLATFORM : AbstractOracleDBTable<PLATFORM.Row>
{
    protected override void InitVars()
    {
        this.PID = AddColumn("PID", typeof(int)); // PID	NUMBER(38,0)
        this.BRANCH = AddColumn("BRANCH", typeof(string));
        this.PERSONA = AddColumn("PERSONA", typeof(string)); // PERSONA	VARCHAR2(255 BYTE)
        this.RESOLUTION = AddColumn("RESOLUTION", typeof(int)); // RESOLUTION	NUMBER(38,0)
    }

    public override string[] filters()
    {
        return new string[] { "BRANCH", "PERSONA", "RESOLUTION" };
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

    public int lookup_pid(string branch, string persona, int resolution)
    {
        Row platform = ((Row)(base.NewRow()));
        platform.BRANCH = branch;
        platform.PERSONA = persona;
        platform.RESOLUTION = resolution;

        platform = findSingleResult(platform);
        if (null != platform)
        {
            return platform.PID;
        }
        return -1;
    }

    public class Row : AbstractDataRow
    {
        private PLATFORM table;
        internal Row(DataRowBuilder rb) : base(rb)
        {
            this.table = ((PLATFORM)(this.Table));
        }

        #region Properties
        public int PID
        {
            get { return ToInteger(table.PID); }
            set { this[table.PID] = value; }
        }
        public string BRANCH
        {
            get { return ToString(table.BRANCH); }
            set { this[table.BRANCH] = value; }
        }
        public string PERSONA
        {
            get { return ToString(table.PERSONA); }
            set { this[table.PERSONA] = value; }
        }
        public int RESOLUTION
        {
            get { return ToInteger(table.RESOLUTION); }
            set { this[table.RESOLUTION] = value; }
        }
        #endregion
    }

    #region Class Members
    DataColumn PID;       // TGUID	VARCHAR2(32 BYTE)
    DataColumn BRANCH;
    DataColumn PERSONA;
    DataColumn RESOLUTION;
    #endregion
}
