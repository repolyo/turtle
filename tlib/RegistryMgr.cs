using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTUtilities
{
    public static class RegistryMgr
    {
        public static void WriteKey(string name, object value)
        {
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("MeridioMigrationTool");
            key.SetValue(name, value);
            key.Close();
        }

        public static object ReadKey(string name)
        {
            object value = null;
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("MeridioMigrationTool");
            value = key.GetValue(name);
            key.Close();
            return value;
        }
    }
}
