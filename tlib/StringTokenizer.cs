using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

// Author: Christopher Tan <chris.tan@hisoft.com>
namespace TLib
{
    /// <summary>
    /// char[] seps = { ',', '\n', '\r' };
    /// String[] values = s1.Split(seps);
    /// </summary>
    public class StringTokenizer : IEnumerator
    {
        private string _str;
        private char [] _delim;
        private bool _returnDelims;
        private string [] _tokens;
        private int _index = -1;

        /// <summary>
        /// Constructs a string tokenizer for the specified string.
        /// </summary>
        /// <param name="str"></param>
        public StringTokenizer(string str) : this(str, null)
        {
        }

        /// <summary>
        /// Constructs a string tokenizer for the specified string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="delim"></param>
        public StringTokenizer(string str, string delim)
        {
            this._str = str;
            this._delim = ((null != delim) ? delim : " ").ToCharArray();
            this.Reset();
        }
          
        /// <summary>
        /// Constructs a string tokenizer for the specified string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="delim"></param>
        /// <param name="returnDelims"></param>
        public StringTokenizer(string str, string delim, bool returnDelims) : this(str, delim)
        {
            this._returnDelims = returnDelims;
        }

        public void Reverse()
        {
            List<string> rev = new List<string>(_tokens.Reverse());
            _tokens = rev.ToArray();
        }
         
        /// <summary>
        /// Returns the same value as the hasMoreTokens method. It exists so that this class can implement the Enumeration interface.
        /// </summary>
        /// <returns></returns>
        public bool hasMoreElements()
        {
            bool hasElem = false;

            return hasElem;
        }

        #region IEnumerator Members
        // IEnumerable Interface Implementation:
        //   Declaration of the GetEnumerator() method 
        //   required by IEnumerable
        public IEnumerator GetEnumerator()
        {
            return this;
        }

        /// <summary>
        /// Calculates the number of times that this tokenizer's nextToken method can be called before it generates an exception.
        /// The current position is not advanced.
        /// </summary>
        /// <returns>the number of tokens remaining in the string using the current delimiter set.</returns>
        public int Length
        {
            get { return this._tokens.Length; }
        }

        public object Current
        {
            get
            {
                return _tokens[_index];
            }
        }

        public string [] Token
        {
            get
            {
                return _tokens;
            }
        }

        public bool MoveNext()
        {
            return (++_index >= _tokens.Length) ? false : true;
        }

        public void Reset()
        {
            this._index = -1;
            this._tokens = this._str.Split(this._delim);
        }
        #endregion
    }
}
