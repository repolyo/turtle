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
        this.SOURCE_FILE = AddColumn("SOURCE_FILE", typeof(int));
        this.LINE_NO = AddColumn("LINE_NO", typeof(int));
        this.FUNC_NAME = AddColumn("FUNC_NAME", typeof(string));
    }

    public override string[] filters()
    {
        return new string[] { "FID", "SOURCE_FILE" };
    }

    public new Row NewRow()
    {
        Row newRow = ((Row)(base.NewRow()));
        return newRow;
    }

    public class Row : AbstractDataRow
    {
        private FUNC table;
        internal Row(DataRowBuilder rb)
            : base(rb)
        {
            this.table = ((FUNC)(this.Table));
        }

        #region Properties
        public string FUNC_NAME
        {
            get { return this[table.FUNC_NAME].ToString(); }
            set { this[table.FUNC_NAME] = value; }
        }
        public int LINE_NO
        {
            get { return Int32.Parse (this[table.LINE_NO].ToString()); }
            set { this[table.LINE_NO] = value; }
        }
        public int FID
        {
            get { return Int32.Parse (this[table.FID].ToString()); }
            set { this[table.FID] = value; }
        }
        public string SOURCE_FILE
        {
            get { return this[table.SOURCE_FILE].ToString(); }
            set { this[table.SOURCE_FILE] = value; }
        }
        #endregion
    }

    #region Class Members
    DataColumn FUNC_NAME;
    DataColumn LINE_NO;
    DataColumn FID;
    DataColumn SOURCE_FILE;
    #endregion
}