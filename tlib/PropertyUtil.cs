using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using CTUtilities.Interfaces;
using CTUtilities.Logging;

namespace CTUtilities
{
    public static class PropertyUtil
    {

        public static object GetProperty(object obj, string prop)
        {
            try
            {
                PropertyInfo info = obj.GetType().GetProperty(prop);
                return info.GetValue(obj, null) ?? string.Empty;
            }
            catch (Exception e)
            {
                Logger.WriteLog(e);
                return string.Empty;
            }
        }

        public static void SetProperty(object obj, string prop, object value)
        {
            try
            {
                PropertyInfo info = obj.GetType().GetProperty(prop);
                info.SetValue(obj, value, null);
            }
            catch (Exception e)
            {
                Logger.WriteLog(e);
            }
        }

        public static IAttribute GetAttribute(DataRow dr)
        {
            DataTable dt = dr.Table;
            IAttribute attrs = (IAttribute)dt.ExtendedProperties[dr];
            if (null == attrs)
            {
                attrs = new HashTableEx();
                dt.ExtendedProperties[dr] = attrs;
            }
            return attrs;
        }

        public static object GetAttributeValue(IAttribute attrs, object key, object def = null)
        {
            return (null == attrs[key]) ? attrs[key] = def : attrs[key];
        }
    }
}
