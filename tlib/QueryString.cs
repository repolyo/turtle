using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TLib.Dao
{
    public class QueryString
    {
        private string str;
        public QueryString(string s) { this.str = s;  }

        public override string ToString()
        {
            return str.Replace("'", "''");
        }
    }
}
