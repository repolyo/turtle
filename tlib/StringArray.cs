using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TLib.Interfaces;

namespace TLib
{
    public class StringArray : List<string>, IAttributeBase<int, string>, IEquatable<StringArray>
    {
        public const char DELIM = ',';

        public StringArray() : base() { }
        public StringArray(string array)
            : base()
        {
            StringTokenizer tokens = new StringTokenizer(array, DELIM.ToString());
            foreach (string t in tokens)
            {
                this.Add(t);
            }
        }

        /// <summary>
        /// Replace value at given position.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string setValueAt(int index, string value)
        {
            if (index < this.Count)
            {
                base.RemoveAt(index);
                base.Insert(index, value);
            }
            return null;
        }

        //public static bool Contains(this string original, string value, StringComparison comparisionType)
        //{
        //    return original.IndexOf(value, comparisionType) >= 0;
        //}

        public bool ContainsIgnoreCase(string value)
        {
            bool contain = false;
            foreach (string item in this)
            {
                if (item.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    contain = true;
                    break;
                }
            }
            return contain;
        }

        public string Flatten(char delim = DELIM)
        {
            string str = string.Empty;
            foreach (string item in this)
            {
                if (string.IsNullOrEmpty(item)) continue;
                str += string.Format("{0}{1}", item, delim);
            }

            return str.TrimEnd(delim);
        }

        public override string ToString()
        {
            return this.Flatten();
        }

        public int GetHashCode(StringArray obj)
        {
            throw new NotImplementedException();
        }

        public bool Equals(StringArray other)
        {
            bool ret = (this.Count == other.Count);
            if (ret)
            {
                for (int i = 0; i < this.Count; i++ )
                {
                    if (null == this[i] || null == other[i]) break;

                    if (!this[i].Equals(other[i]))
                    {
                        ret = false;
                        break;
                    }
                }
            }
            return ret;
        }
    }
}
