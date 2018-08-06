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
    internal virtual void InitVars()
    {
        this.function_id = AddColumn("FID", typeof(int));
        this.source_file = AddColumn("SOURCE_FILE", typeof(int));
        this.line_no = AddColumn("LINE_NO", typeof(int));
        this.function_name = AddColumn("FUNC_NAME", typeof(string));
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

    public class Row : DataRow
    {
        private FUNC table;
        internal Row(DataRowBuilder rb)
            : base(rb)
        {
            this.table = ((FUNC)(this.Table));
        }

        #region Properties
        public string function_name
        {
            get { return this[table.function_name].ToString(); }
            set { this[table.function_name] = value; }
        }
        public int line_no
        {
            get { return (int)this[table.line_no]; }
            set { this[table.line_no] = value; }
        }
        public int function_id
        {
            get { return (int)this[table.function_id]; }
            set { this[table.function_id] = value; }
        }
        public string source_file
        {
            get { return this[table.source_file].ToString(); }
            set { this[table.source_file] = value; }
        }
        #endregion
    }

    #region Class Members
    DataColumn function_name;
    DataColumn line_no;
    DataColumn function_id;
    DataColumn source_file;
    #endregion
}