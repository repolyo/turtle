using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Tlib;
using TLib;

namespace Tlib.Dao
{
    public abstract class AbstractDataRow : DataRow
    {
        private Range<int> range;

        protected AbstractDataRow(DataRowBuilder rb)
            : base(rb)
        {
            range = null;
        }

        public virtual List<string> Columns() { return null;  }

        public object getValue(DataColumn col)
        {
            return this.IsNull(col) ? null : this[col];
        }

        protected int ToInteger(DataColumn col)
        {
            if (this.IsNull(col))
                return 0;

            return Int32.Parse(this[col].ToString());
        }

        protected bool ToBoolean(DataColumn col)
        {
            if (this.IsNull(col))
                return false;

            return Boolean.Parse(this[col].ToString());
        }

        protected string ToString(DataColumn col)
        {
            return this.IsNull(col) ? null : this[col].ToString();
        }

        #region Properties
        public Range<int> Range
        {
            get { return range; }
            set { this.range = value; }
        }
        #endregion
    }
}
