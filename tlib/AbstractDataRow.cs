using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MeridioTools.dao
{
    public abstract class AbstractDataRow : DataRow
    {
        protected AbstractDataRow(DataRowBuilder rb)
            : base(rb)
        {
        }

        public object getValue(DataColumn col)
        {
            return this.IsNull(col) ? null : this[col];
        }
    }
}
