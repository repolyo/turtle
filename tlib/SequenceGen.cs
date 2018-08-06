using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CTUtilities
{
    public class SequenceGen
    {
        private uint value = uint.MinValue;

        private static SequenceGen instance = null;
        private SequenceGen() { }
        public static SequenceGen getInstance()
        {
            if (null == instance) instance = new SequenceGen();
            // return string.Format("{0}", --sequenceId);
            return instance;
        }
        public int Value
        {
            get { return (int)this.value; }
        }
        public int NextVal
        {
          get { return (int)++this.value; }
          set { this.value = (uint)value; }
        }
    }
}
