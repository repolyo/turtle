using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using TLib.Logging;

namespace TLib
{
    namespace Exceptions
    {
        public abstract class AbstractBaseException : Exception
        {
            public abstract LogLevel Level
            {
                get;
                set;
            }

            protected AbstractBaseException() : base() { }
            protected AbstractBaseException(string factoryName, string msg, Exception e = null) : base(string.Format("{0} -- {1}", factoryName, msg), e) { }
            protected AbstractBaseException(string err, params object[] args) : base(string.Format(err, args)) { } 
            protected AbstractBaseException(Exception e, string fmt, params object[] args) 
                : base(string.Format(fmt, args), e) { this.Level = LogLevel.ERROR; }

            public AbstractBaseException Add(Exception e)
            {
                return this;
            }
        }

        public abstract class AbstractException<T> : AbstractBaseException
        {
            private LogLevel level = LogLevel.ERROR;
            public override LogLevel Level
            {
                get { return level; }
                set { level = value; }
            }

            protected DataRow dr;
            //public AbstractException(object source, DataRow dr, Exception e = null)
            //    : this(
            //        source.ToString(),
            //        string.Format("Failed creation of {0}/{1}",
            //            dr[FILES_Factory.PREFIX],
            //            dr[FILES_Factory.FILE_NUMBER]),
            //        e)
            //{
            //    this.dr = dr;
            //}
            protected AbstractException(string f, string msg, Exception e = null) : base(f, msg, e) { }

            public AbstractException(object source, string msg, Exception e = null)
                : base(source.ToString(), msg, e)
            {
            }

            protected AbstractException() : base() { this.Level = LogLevel.WARN; }
            protected AbstractException(Exception e, string err, params object[] args) : base(e, err, args) { } 
            protected AbstractException(Exception e) : base(e, "") { }
            protected AbstractException(string err, params object[] args) : base(err, args) { } 

        }

        public class OperationCanceledExceptionEx : OperationCanceledException
        {
            public OperationCanceledExceptionEx(string err, params object[] args) : this(null, err, args) { }
            public OperationCanceledExceptionEx(Exception e, string err, params object[] args) : base(string.Format(err, args), e) { } 
        }

        public class DBInsertException<T> : AbstractException<T>
        {
            public DBInsertException() : base() { }
            public DBInsertException(Exception e) : base(e) { }
            public DBInsertException(string err, params object[] args) : base(err, args) { }
            public DBInsertException(Exception e, string err, params object[] args) : base(e, err, args) { } 
        }

        public class DBConnectionException<T> : AbstractException<T>
        {
            public DBConnectionException() : base() { }
            public DBConnectionException(Exception e) : base(e) { }
            public DBConnectionException(string err, params object[] args) : base(err, args) { }
            public DBConnectionException(Exception e, string err, params object[] args) : base(e, err, args) { }
        }

        public class NotFoundException<T> : AbstractException<T>
        {
            public NotFoundException() : base() { }
            public NotFoundException(Exception e) : base(e) { }
            public NotFoundException(string err, params object[] args) : base(err, args) { }
            public NotFoundException(Exception e, string err, params object[] args) : base(e, err, args) { }
        }

        public class MultipleMatchException<T> : AbstractException<T>
        {
            public MultipleMatchException() : base() { }
            public MultipleMatchException(Exception e) : base(e) { }
            public MultipleMatchException(string err, params object[] args) : base(err, args) { }
            public MultipleMatchException(Exception e, string err, params object[] args) : base(e, err, args) { }
        }

        internal class DateException : Exception
        {
            public DateException(string err, params object[] args) : base(string.Format(err, args)) { } 
        }
    }
}
