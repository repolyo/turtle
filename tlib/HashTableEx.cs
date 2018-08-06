using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml.Serialization;
using TLib.Interfaces;

namespace TLib
{
    public class HashTableEx : Hashtable, IAttribute
    {
        // Summary:
        //     Gets or sets the value associated with the specified key.
        //
        // Parameters:
        //   key:
        //     The key whose value to get or set.
        //
        // Returns:
        //     The value associated with the specified key. If the specified key is not
        //     found, attempting to get it returns null, and attempting to set it creates
        //     a new element using the specified key.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        //
        //   System.NotSupportedException:
        //     The property is set and the System.Collections.Hashtable is read-only.  -or-
        //     The property is set, key does not exist in the collection, and the System.Collections.Hashtable
        //     has a fixed size.
        //public override object this[object key]
        //{
        //    get
        //    {
        //        return base[key];
        //    }
        //    set
        //    {
        //        base[key] = value;
        //    }
        //}
        public object get(object key, object defvalue = null)
        {
            object ret = base[key];
            return (null == ret) ? base[key] = defvalue : ret;
        }


        public void Copy(IAttribute attrs)
        {
            foreach (object key in attrs.Keys)
            {
                this[key] = attrs[key];
            }
        }
    }
}
