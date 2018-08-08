using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Data.Common;
using System.Reflection;
using System.Globalization;
using System.Text.RegularExpressions;
using System.ComponentModel;
using TLib;
using TLib.Interfaces;
using TLib.Container;
using TLib.Logging;
using TLib.Exceptions;
using TLib.Dao;
using System.IO;

///  @author Christopher Tan
///  @email chris.tan@beansgrp.com
namespace Tlib.Dao
{
    /// <summary>
    ///  @author Christopher Tan
    ///  @email chris.tan@beansgrp.com
    /// </summary>
    /// <typeparam name="E"></typeparam>
    public abstract class AbstractTableDB<E> : DataTable, SQLCmd<E>, IAttributeBase<object, object>
    {
        protected const string SELECT = "SELECT";
        protected const string FROM = "FROM";
        protected const string WHERE = "WHERE";
        public const string COUNT = "COUNT";
        protected bool asc = true;
        //protected IDbConnection2 con;
        protected DataTable insideTbl;
        private BackgroundWorkerEx thread;
        public BackgroundWorkerEx Thread
        {
            get { return thread; }
            set { thread = value; }
        }

        public AbstractTableDB(DataTable tbl)
        {
            if (null != tbl)
            {
                this.insideTbl = tbl;
                this.TableName = tbl.TableName;
                foreach (DataColumn c in tbl.Columns)
                {
                    this.Columns.Add(c.ColumnName, c.DataType);
                }
            }
        }

        protected AbstractTableDB()
        {
            this.TableName = this.GetType().Name;
            
            // call this prior to initialize, we will load DB columns in real time.
            if (!string.IsNullOrEmpty(this.TableName))
            {
                this.findAll(string.Format("SELECT * FROM {0} WHERE 1<>1",
                    this.TableName));
            }

            this.initialize();
        }

        public virtual void initialize()
        {
            this.primaryKeys.Clear();
            this.InitVars();
            this.PrimaryKey = primaryKeys.ToArray();
        }

        protected virtual void InitVars() { /*throw new NotImplementedException();*/  }

        List<DataColumn> primaryKeys = new List<DataColumn>();
        protected DataColumn AddColumn(string name, Type type, bool primary = false)
        {
            //this.prefix = this.Columns[PREFIX];
            DataColumn col = this.Columns[name];
            if (null == col)
            {
                col = new DataColumn(name, type, null, global::System.Data.MappingType.Element);
                base.Columns.Add(col);
            }
            if (primary) primaryKeys.Add(col);
            //Console.WriteLine("{0}[{1}] = {2}", name, col.DataType.ToString(), type.ToString());

            return col;
        }

        //public new E NewRow()
        //{
        //    E newRow = ((E)(base.NewRow()));
        //    return newRow;
        //}

        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            //return new E(builder);
            return (DataRow)Activator.CreateInstance(typeof(E), builder);
        }

        //protected override System.Type GetRowType()
        //{
        //    return typeof(E);
        //}
        protected override Type GetRowType()
        {
            return typeof(E);
        }

        public void RemoveRow(DataRow row)
        {
            this.Rows.Remove(row);
        }

        protected virtual IPool<IPoolable> GetConnectionPool() { return null; }
        protected virtual AbstractDbConnection<DbConnection> NewConnection() { return null; }
        protected virtual DataTable Table() { return this; }

        /// <summary>
        /// Removes all records in associated remote DB.
        /// </summary>
        public virtual bool deleteAll(int userId) 
        {
            this.delete(string.Empty);
            return false; 
        }

        public object this[object key]
        {
            get { return ExtendedProperties[key]; }
            set { ExtendedProperties[key] = value; }
        }

        public AbstractTableDB<E> Copy(DataTable tbl)
        {
            this.TableName = tbl.TableName;
            this.BeginLoadData();
            foreach (DataRow newDR in tbl.Rows)
            {
                //this.LoadDataRow(newDR.ItemArray, true);
                this.ImportRow(newDR);
            }
            this.EndLoadData();
            return this;
        }

        protected virtual bool debug() { return false; }
        //protected abstract E create(DataRow dr);

        protected abstract IDbCommand2 newCommand(string sql);

        /// <summary>
        /// returns the search criteria when querying to database.
        /// search criteria is in the form Map<String, String>.
        /// where Map<alias, actual key>.
        /// @return search criteria db table attribute mapping.
        /// </summary>
        /// <returns></returns>
        public abstract string[] filters();
        public virtual List<DataColumn> getDataColumns(string[] filters)
        {
            List<string> filter = new List<string>(filters);
            TableColumns cols = new TableColumns();
            foreach (DataColumn col in this.Columns)
            {
                if (filter.Contains(col.ColumnName)) cols.Add(col);
            }
            return cols;
        }

        protected virtual string[] sort()
        {
            return new string[] { };
        }

        protected class StringArraySQL : StringArray
        {
            public override string ToString()
            {
                return trimSQL(base.ToString());
            }
        }

        protected class TableColumns : List<DataColumn>
        {
            public override string ToString()
            {
                return Flatten(char.MaxValue);
            }

            public string Flatten(char val = char.MaxValue)
            {
                String str = string.Empty;
                foreach (DataColumn col in this)
                {
                    str += string.Format("{0}, ", (char.MaxValue != val) ? val.ToString() : col.ColumnName);
                }

                return trimSQL(str);
            }

            public string ValuesToString()
            {
                String str = string.Empty;
                foreach (DataColumn col in this)
                {
                    object value = col.DefaultValue;
                    if (value is string)
                    {
                        str += string.Format("'{0}', ", value.ToString());
                    }
                    else
                    {
                        str += string.Format("{0}, ", value);
                    }
                }

                return trimSQL(str);
            }

            public string Flatten(string prefix, string suffix)
            {
                String str = string.Empty;
                foreach (DataColumn col in this)
                {
                    str += string.Format("{0}{1}{2}, ", prefix, col.ColumnName, suffix);
                }

                return trimSQL(str);
            }

            public string Remove(string col)
            {
                DataColumn rem = this.Find(delegate(DataColumn dc)
                {
                    return dc.ColumnName == col;
                });

                if (null != rem) base.Remove(rem);
                return col;
            }

            public static string Format(DataColumn dc, object value)
            {
                return AbstractTableDB<object>.Format(dc.ColumnName, value);
            }

            public string FormattedColumnValuePair(string column)
            {
                DataColumn field = this.Find(delegate(DataColumn dc)
                {
                    return dc.ColumnName == column;
                });
                if (field != null) {
                    return Format(field, field.DefaultValue);
                }
                return string.Empty;
            }
        }

        public static string Format(string col, object value)
        {
            string fmt = string.Empty;
            if (value is string)
            {
                fmt = string.Format("{0} = '{1}'", col, new QueryString((string)value));
            }
            else
            {
                fmt = string.Format("{0} = {1}", col, value);
            }
            return fmt;
        }

        static string trimSQL(string sql)
        {
            return sql.TrimEnd(new char[] { ',', ' ' });
        }

        protected virtual TableColumns columns(E filter)
        {
            object value = filter;
            TableColumns cols = new TableColumns();
            foreach (DataColumn col in this.Columns)
            {
                if (!PrimaryKey.Contains(col))
                {
                    string key = col.ColumnName;
                    object val = getFieldValue(col, filter);

                    if (null == val) continue;

                    col.DefaultValue = val;
                    cols.Add(col);
                }
            }
            return cols;
        }

        /// <summary>
        /// Converts the given "OADate" (OLE Automation Date) in string format
        /// to equivalent DateTime value.
        /// </summary>
        /// <param name="oaDate"></param>
        /// <returns></returns>
        protected DateTime convertOADate(object oaDate)
        {
            DateTime newDate;
            string val = Converter.ToString(oaDate);
            if (!string.IsNullOrEmpty(val) && !val.Contains("/"))
            {
                double oaValue = double.Parse(val);
                newDate = DateTime.FromOADate(oaValue);
            }
            else
            {
                newDate = Convert.ToDateTime(oaDate);
            }

            return newDate;
        }

        /// <summary>
        /// 25:07:2011 18:18:46
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected DateTime ToDateTime(object date)
        {
            DateTime parsedDate = DateTime.Now;
            DateTime.TryParseExact(date.ToString(), "dd:MM:yyyy HH:mm:ss", null, DateTimeStyles.None, out parsedDate);

            return parsedDate;
        }

        protected object getFieldValue(string field, E obj)
        {
            object value = obj;
            value = null;

            foreach (DataColumn dc in this.Columns)
            {
                if (dc.ColumnName == field)
                {
                    value = getFieldValue(dc, obj);
                    break;
                }
            }

            return value;
        }

        protected object getFieldValue(DataColumn field, E obj)
        {
            object value = obj;
            value = null;

            try
            {
                PropertyInfo prop = obj.GetType().GetProperty(field.ColumnName);
                if (null != prop)
                {
                    object tmpVal = prop.GetValue(obj, null);
                    if (null != tmpVal && DBNull.Value != tmpVal)
                    {
                        value = Converter.Format(tmpVal, Type.GetTypeCode(field.DataType));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return value;
        }

        public string whereSQL(E filter, List<DataColumn> where)
        {
            string whereSQL = string.Empty;
            if (where.Count > 0)
            {
                foreach (DataColumn field in where)
                {
                    object value = getFieldValue(field, filter);
                    if (null != value)
                    {
                        whereSQL += string.Format("{0},", TableColumns.Format(field, value));
                    }
                }
            }
            return trimSQL(whereSQL).Replace(",", " AND ");
        }

        protected string buildSelectSQL(E filter, StringArraySQL cols = null, List<DataColumn> where = null, List<DataColumn> sorting = null)
        {
            //if (null == cols)
            //{
            //    cols = new StringArraySQL();
            //    foreach (DataColumn col in this.Columns)
            //    {
            //        switch (Type.GetTypeCode(col.DataType))
            //        {
            //            case TypeCode.String:
            //                //cols.Add(string.Format("TRIM({0})", col.ColumnName));
            //                cols.Add(col.ColumnName);
            //                break;
            //            default:
            //                cols.Add(col.ColumnName);
            //                break;
            //        }
            //    }
            //}

            String sql = string.Format("{0} {1} {2} {3}", SELECT, (null == cols) ? "*" : cols.ToString(), FROM, this.TableName);
            if (null != where && where.Count > 0)
            {
                string whereSQL = string.Empty;
                foreach (DataColumn field in where)
                {
                    object value = getFieldValue(field, filter);
                    if (null != value)
                    {
                        whereSQL += string.Format("{0},", TableColumns.Format(field, value));
                    }
                }
                if (!string.IsNullOrEmpty(whereSQL))
                {
                    sql += string.Format(" {0} {1}", WHERE, trimSQL(whereSQL).Replace(",", " AND "));
                }
            }

            if (null != sorting && sorting.Count > 0)
            {
                string sortSQL = string.Empty;
                foreach (DataColumn field in sorting)
                {
                    sortSQL += string.Format("{0},", field.ColumnName);
                }

                if (!string.IsNullOrEmpty(sortSQL))
                {
                    sql += string.Format(" ORDER BY {0} {1}", trimSQL(sortSQL), asc ? "ASC" : "DESC");
                }

            }

            return sql;
        }

        public long peekResultCount(E filter)
        {
            int ret = 0;
            StringArraySQL args = new StringArraySQL();
            args.Add("COUNT(*)");
            String sql = buildSelectSQL(filter, args);

            using (IDbCommand2 cmd = newCommand(sql))
            {
                ret = Convert.ToInt32(cmd.ExecuteScalar());
                Logger.WriteLine("{0} = {1}", sql, ret);
            }
            return ret;
        }

        public DataTable findAll(E filter)
        {
            DataTable dt = null;
            List<DataColumn> where = getDataColumns(this.filters());
            List<DataColumn> sorting = getDataColumns(this.sort());
            String sql = buildSelectSQL(filter, null, where, sorting);
            using (IDbCommand2 cmd = newCommand(sql))
            {
                this.Clear();
                dt = cmd.findAll(this.Table());
            }
            return dt;
        }

        public DataTable findAll(DataTable dt, E filter)
        {
            List<DataColumn> where = getDataColumns(this.filters());
            List<DataColumn> sorting = getDataColumns(this.sort());
            String sql = buildSelectSQL(filter, null, where, sorting);
            using (IDbCommand2 cmd = newCommand(sql))
            {
                dt.Clear();
                dt = cmd.findAll(dt);
            }
            return dt;
        }

        public E find(E filter)
        {
            object ret = null;
            StringArraySQL cols = new StringArraySQL();
            foreach (DataColumn dc in this.Columns)
            {
                cols.Add(dc.ColumnName);
            }

            List<DataColumn> where = getDataColumns(this.filters());
            List<DataColumn> sorting = getDataColumns(this.sort());
            String sql = buildSelectSQL(filter, null, where, sorting);
            using (IDbCommand2 cmd = newCommand(sql))
            {
                this.Clear();
                this.Load(cmd.ExecuteReader());
                switch (this.Rows.Count)
                {
                    case 0:
                        throw new NotFoundException<E>(sql);
                    case 1:
                        ret = this.Rows[0];
                        break;
                    default:
                        throw new MultipleMatchException<E>(sql);
                }
            }
            return (E)ret;
        }

        public virtual E findSingleResult(E filter)
        {
            return find(filter);
        }

        public E insert(E rec)
        {
            TableColumns cols = this.columns(rec);
            cols.Remove("OBJECT");

            string sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                this.TableName,
                cols.ToString(),
                cols.Flatten("@", string.Empty));

            using (IDbCommand2 cmd = newCommand(sql))
            {
                foreach (DataColumn c in cols)
                {
                    string key = c.ColumnName;
                    object value = getFieldValue(c, rec);
                    if (null != value)
                    {
                        //Console.WriteLine("command.Parameters({0}, {1});", key, value);
                        cmd.AddWithValue(string.Format("@{0}", key), value);
                    }
                }

                if (1 != cmd.ExecuteNonQuery())
                {
                    throw new DBInsertException<E>(sql);
                }
            }
            return rec;
        }

        public E merge(E rec)
        {
            string sql = string.Empty;
            TableColumns cols = this.columns(rec);
            List<DataColumn> where = getDataColumns(this.filters());
            cols.Remove("OBJECT");

            sql = string.Format("MERGE INTO {0} USING DUAL ON ({1}) WHEN NOT matched THEN INSERT ({2}) VALUES ({3}) WHEN matched then UPDATE SET {4}",
                this.TableName,
                whereSQL(rec, where),
                cols.ToString(),
                cols.ValuesToString(),
                cols.FormattedColumnValuePair ("CHECKSUM"));

            using (IDbCommand2 cmd = newCommand(sql))
            {
                if (1 != cmd.ExecuteNonQuery())
                {
                    throw new DBInsertException<E>(sql);
                }
            }
            return rec;
        }

        private class SqlKeyValuePair
        {
            private object key;
            private object value;
            private int index;

            public SqlKeyValuePair(object k, object v, int i)
            {
                this.key = k;
                this.value = v;
                this.index = i;
            }

            public override string ToString()
            {
                StringBuilder b = new StringBuilder();
                b.Append(key);
                b.Append(" = {");
                b.Append(index);
                b.Append('}');
                return b.ToString();
            }
        }

        class SqlCommandString
        {
            private List<object> args = new List<object>();
            public object [] Arguments
            {
                get { return args.ToArray(); }
            }
            private StringBuilder sql = new StringBuilder();
            public SqlCommandString(string fmt, params object[] args)
            {
                sql.AppendFormat(fmt, args);
            }

            public void Append(string fmt, params object[] args)
            {
                sql.AppendFormat(fmt, args);
            }

            public void AddParam(object k, object v, char delim = ' ')
            {
                if (0 == args.Count) delim = ' ';
                sql.AppendFormat("{0}{1} = ", delim, k);
                sql.Append('{');
                sql.Append(args.Count);
                sql.Append('}');
                args.Add(v);
            }

            public override string ToString()
            {
                return sql.ToString();
            }
        }

        public virtual E update(E rec)
        {
            string [] filter = this.filters();
            List<DataColumn> where = getDataColumns(filter);
            List<DataColumn> sorting = getDataColumns(this.sort());
            List<object> args = new List<object>();
            TableColumns cols = this.columns(rec);
            SqlCommandString sql = new SqlCommandString("UPDATE {0} SET ", this.TableName);

            foreach (DataColumn c in cols)
            {
                string key = c.ColumnName;
                if (filter.Contains(key))
                {
                    continue;
                    //throw new SQLSyntaxErrorException("cannot update primary key!!!");
                }

                object value = getFieldValue(c, rec);
                if (null != value)
                {
                    sql.AddParam(key, value, ',');
                }
            }
            if (null != where && where.Count > 0)
            {
                sql.Append(" WHERE ");
                bool first = true;
                foreach (DataColumn field in where)
                {
                    string key = field.ColumnName;
                    object value = getFieldValue(field, rec);
                    if (null != value)
                    {
                        sql.AddParam(key, value, (first)?' ' : ',');
                        first = false;
                    }
                }
            }

            this.update(sql.ToString(), sql.Arguments);
            return rec;
        }

        public virtual E delete(E rec)
        {
            throw new NotImplementedException();
        }

        public virtual E newObject(string prefix=null)
        {
            Type t = typeof(E);
            return (E)Activator.CreateInstance(t);
        }

        public virtual DataTable findAll()
        {
            return this.findAll( newObject() );
        }

#if FALSE
        public static string FormatWith(this string format, IFormatProvider provider, object source)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            Regex r = new Regex(@"(?<start>\{)+(?<property>[\w\.\[\]]+)(?<format>:[^}]+)?(?<end>\})+",
              RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            List<object> values = new List<object>();
            string rewrittenFormat = r.Replace(format, delegate(Match m)
            {
                Group startGroup = m.Groups["start"];
                Group propertyGroup = m.Groups["property"];
                Group formatGroup = m.Groups["format"];
                Group endGroup = m.Groups["end"];

                values.Add((propertyGroup.Value == "0")
                  ? source
                  : DataBinder.Eval(source, propertyGroup.Value));

                return new string('{', startGroup.Captures.Count) + (values.Count - 1) + formatGroup.Value
                  + new string('}', endGroup.Captures.Count);
            });

            return string.Format(provider, rewrittenFormat, values.ToArray());
        }
#endif

        /// <summary>
        /// parses given sql statement traversing and identifying any set values.
        /// set values would be associated in then sequence of given args objects...
        /// that is, <set value 1> = args[1], <set value 2> = args[2]... and so forth.
        /// 	  	<any string> = :<set value>
        ///		    <any string> <> :<set value>
        ///		    <any string> like :<set value>
        /// e.g set value format.
        /// </summary>
        /// <param name="sql">query statement</param>
        /// <param name="args">arguments</param>
        /// <returns></returns>
        public HashTableEx parseJPQL(ref string sql, params object[] args)
        {
            Regex r = new Regex(@"(?<start>\{)+(?<property>[\w\.\[\]]+)(?<format>:[^}]+)?(?<end>\})+",
              RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            HashTableEx values = new HashTableEx();
            string rewrittenFormat = r.Replace(sql, delegate(Match m)
            {
                Group startGroup = m.Groups["start"];
                Group propertyGroup = m.Groups["property"];
                Group formatGroup = m.Groups["format"];
                Group endGroup = m.Groups["end"];

                string key = string.Format("@{0}", propertyGroup.Value);
                object value = args[Convert.ToInt32(propertyGroup.Value)];
                //values.Add((propertyGroup.Value == "0") ? cmd : DataBinder.Eval(cmd, propertyGroup.Value));
                values[key] = value;

                return key;
            });
            if (values.Count > args.Length)
            {
                throw new IndexOutOfRangeException("Insufficient SQL argument");
            }
            sql = rewrittenFormat;
            return values;
        }

        protected IDbCommand2 setQueryParameters(IDbCommand2 cmd, HashTableEx prop)
        {
            foreach (string key in prop.Keys)
            {
                cmd.AddWithValue(key, prop[key]);
            }

            return cmd;
        }

        protected IDbCommand2 createQuery(String sql, HashTableEx prop)
        {
            IDbCommand2 q = newCommand(sql);
            return setQueryParameters(q, prop);
        }

        public virtual int insert(string sql, params object[] args)
        {
            HashTableEx prop = parseJPQL(ref sql, args);
            using (IDbCommand2 cmd = createQuery(sql, prop))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public HashTableEx storedProcedureParams(ref string fmt, HashTableEx args)
        {
            HashTableEx newProp = new HashTableEx();
            StringTokenizer tokenizer = new StringTokenizer(fmt, ",");
            //foreach (string token in tokenizer)
            //{
            //    int end = fmt.IndexOf("=");
            //    int start = end; while (start > 0 && (fmt[start] != ' ' || fmt[start] != ',')) start--;
            //    string name = fmt.Substring(start, end - start);

            foreach (string key in args.Keys)
                {
                    //string regex = @"(?<name>\="+key+")";
                    //Match match = Regex.Match(fmt, regex,
                    //    RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
                    //// Here we check the Match instance.
                    //if (match.Success)
                    //{
                    //    object name = match.Groups[2].Value;
                    //    newProp[name] = args[key];
                    //}
                    int end = fmt.IndexOf(key) - 1;
                    int start = end; while (start > 0 && (fmt[start] != '@')) start--;
                    string name = fmt.Substring(start, end - start);
                    newProp[name] = args[key];
                }
            //}
            return newProp;
        }

        public virtual int callStoredProcedure(string name)
        {
            return callStoredProcedure(name, "", new object[]{});
        }

        public virtual int callStoredProcedure(string name, string param, params object[] args)
        {
            HashTableEx prop = parseJPQL(ref param, args);
            prop = storedProcedureParams(ref param, prop);
            using (IDbCommand2 cmd = newCommand(name))
            {
                this.setQueryParameters(cmd, prop);
                cmd.CommandText = name;
                cmd.CommandType = CommandType.StoredProcedure;
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="sql">SQL command</param>
        /// <param name="args">sequence of objec associated with @param in given sql</param>
        /// <returns></returns>
        public virtual int update(string sql, params object[] args)
        {
            HashTableEx prop = parseJPQL(ref sql, args);
            using (IDbCommand2 cmd = createQuery(sql, prop))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public virtual E singleResultQuery(string sql, params object[] args)
        {
            object ret = find(sql, args);
            return (E)ret;
        }

        public virtual DataRow find(string sql, params object[] args)
        {
            DataRow ret = null;
            HashTableEx prop = parseJPQL(ref sql, args);
            using (IDbCommand2 cmd = createQuery(sql, prop))
            {
                this.Clear();
                this.Load(cmd.ExecuteReader());
                if (this.Rows.Count == 1)
                {
                    ret = (DataRow)create(this.Rows[0]);
                }
            }

            return ret;
        }

        public virtual E query(string fmt, params object[] args)
        {
            object ret = null;
            string sql = string.Format("SELECT * FROM {0} WHERE {1}", this.TableName, fmt);
            HashTableEx prop = parseJPQL(ref sql, args);
            using (IDbCommand2 cmd = createQuery(sql, prop))
            {
                this.Clear();
                this.Load(cmd.ExecuteReader());
                ret = (this.Rows.Count == 1) ? this.Rows[0] : null;
            }
            return (E)ret;
        }

        public virtual object delete(string fmt, params object[] args)
        {
            StringBuilder tmpSQL = new StringBuilder(string.Format("DELETE FROM {0} ", this.TableName));
            if (args.Length > 0)
            {
                tmpSQL.AppendFormat("WHERE {0}", fmt);
            }
            string sql = tmpSQL.ToString();
            HashTableEx prop = parseJPQL(ref sql, args);
            using (IDbCommand2 cmd = createQuery(sql, prop))
            {
                int ret = cmd.ExecuteNonQuery();
                return null;
            }
        }

        public virtual object executeSQL(string sql, params object[] args)
        {
            HashTableEx prop = parseJPQL(ref sql, args);
            using (IDbCommand2 cmd = createQuery(sql, prop))
            {
                int ret = cmd.ExecuteNonQuery();
                return null;
            }
        }

        public virtual object ExecuteScalar(string sql, params object[] args)
        {
            Console.WriteLine(string.Format(sql, args));
            HashTableEx prop = parseJPQL(ref sql, args);
            using (IDbCommand2 cmd = createQuery(sql, prop))
            {
                return cmd.ExecuteScalar();
            }
        }

        public virtual DataTable findAll(string sql, params object[] args)
        {
            return findAll(this, sql, args);
        }

        public virtual DataTable findAll(DataTable tbl, string sql, params object[] args)
        {
            string cmdSQL = string.Format(sql, args);
            HashTableEx prop = parseJPQL(ref sql, args);
            using (IDbCommand2 cmd = createQuery(sql, prop))
            {
                tbl.Clear();
                tbl.Load(cmd.ExecuteReader());
            }
            Console.WriteLine("{0} = {1}", cmdSQL, tbl.Rows.Count);
            return tbl;
        }

        public object create(DataRow dr)
        {
            DataRow newRow = insideTbl.NewRow();
            foreach (DataColumn col in insideTbl.Columns)
            {
                newRow[col.ColumnName] = dr[col.ColumnName];
            }

            return newRow;
        }

        public static void ToCSV(DataTable dtDataTable, string strFilePath)
        {
            StreamWriter sw = new StreamWriter(strFilePath, false);
            //headers  
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }  
    }

}

