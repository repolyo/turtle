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
    internal virtual void InitVars()
    {
        this.pid = AddColumn("pid", typeof(int)); // PID	NUMBER(38,0)
        this.persona = AddColumn("persona", typeof(string)); // PERSONA	VARCHAR2(255 BYTE)
        this.resolution = AddColumn("resolution", typeof(int)); // RESOLUTION	NUMBER(38,0)
    }

    public override string[] filters()
    {
        return new string[] { "pid", "persona", "resolution" };
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

    public int lookup_pid(string persona, int resolution)
    {
        Row platform = ((Row)(base.NewRow()));
        platform.persona = persona;
        platform.resolution = resolution;

        platform = find(platform);
        if (null != platform)
        {
            return platform.pid;
        }
        return -1;
    }

    public class Row : DataRow
    {
        private PLATFORM table;
        internal Row(DataRowBuilder rb) : base(rb)
        {
            this.table = ((PLATFORM)(this.Table));
        }

        #region Properties
        public int pid
        {
            get { return (int)this[table.pid]; }
            set { this[table.pid] = value; }
        }
        public string persona
        {
            get { return this[table.persona].ToString(); }
            set { this[table.persona] = value; }
        }
        public int resolution
        {
            get { return (int)this[table.resolution]; }
            set { this[table.resolution] = value; }
        }
        #endregion
    }

    #region Class Members
    DataColumn pid;       // TGUID	VARCHAR2(32 BYTE)
    DataColumn persona;
    DataColumn resolution;
    #endregion
}
