using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TLib
{
    public static class Converter
    {
        //public static int ToInt32(object value)
        //{
        //    return Convert.DBNull.Equals(value) ? value : Convert.ToInt32(value);
        //}

        public static string ToString(object o, string def = "")
        {
            string value = (o != Convert.DBNull) ? (string)Format(o, TypeCode.String) : null;
            return (null != value) ? value.ToString().Trim() : def;
        }

        public static T Format<T>(object value, T def = default(T))
        {
            //T ret = Format(value, Convert.GetTypeCode(value));
            //return (null == ret) ? def : ret;
            return (T)Format(value, (null != def) ? Convert.GetTypeCode(def) : Convert.GetTypeCode(value));
        }

        public static object Format(object value, TypeCode type)
        {
            object ret = null;
            switch (type)
            {
                case TypeCode.DBNull:
                    ret = null;
                    break;
                case TypeCode.String:
                    ret = Convert.ToString(value);
                    break;
                case TypeCode.Int16:
                    ret = Convert.ToInt16(value);
                    break;
                case TypeCode.Int32:
                case TypeCode.Decimal:
                    ret = Convert.ToInt32(value);
                    break;
                case TypeCode.Int64:
                    ret = Convert.ToInt64(value);
                    break;
                case TypeCode.UInt16:
                    ret = Convert.ToUInt16(value);
                    break;
                case TypeCode.UInt32:
                    ret = Convert.ToUInt32(value);
                    break;
                case TypeCode.UInt64:
                    ret = Convert.ToUInt64(value);
                    break;
                case TypeCode.Char:
                    ret = Convert.ToChar(value);
                    break;
                case TypeCode.DateTime:
                    if (value is string) ret = Date.Format(value.ToString());
                    else ret = Date.ToDateTime(value);
                    break;
                case TypeCode.Double:
                    ret = Convert.ToDouble(value);
                    break;
                case TypeCode.Boolean:
                    ret = Convert.ToBoolean(value);
                    break;
                case TypeCode.Byte:
                    ret = Convert.ToByte(value);
                    break;
                case TypeCode.SByte:
                    ret = Convert.ToSByte(value);
                    break;
                case TypeCode.Empty:
                default:
                    ret = null;
                    break;
            }
            return ret;
        }
    }
}
