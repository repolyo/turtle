using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using TLib.Exceptions;
using TLib.Logging;

namespace TLib
{
    /// <summary>
    /// d    2/27/2009
    /// D    Friday, February 27, 2009
    /// f    Friday, February 27, 2009 12:11 PM
    /// F    Friday, February 27, 2009 12:12:22 PM
    /// g    2/27/2009 12:12 PM
    /// G    2/27/2009 12:12:22 PM
    /// m    February 27
    /// M    February 27
    /// o    2009-02-27T12:12:22.1020000-08:00
    /// O    2009-02-27T12:12:22.1020000-08:00
    /// s    2009-02-27T12:12:22
    /// t    12:12 PM
    /// T    12:12:22 PM
    /// u    2009-02-27 12:12:22Z
    /// U    Friday, February 27, 2009 8:12:22 PM
    /// y    February, 2009
    /// Y    February, 2009
    /// </summary>
    public class Date
    {
        public static Date NULL = new Date(default(DateTime));

        //public static implicit operator DateTime(Date date)
        //{
        //    // code to convert from MyType to int
        //    return (null != date) ? date.Value : NULL.Value;
        //}

        public bool isNULL(Date date) { return (NULL == date); }

        private DateTime date = DateTime.Now;
        public DateTime Value
        {
            get { return date; }
            set { date = value; }
        }

        public Date() : this(DateTime.Now)
        {
        }

        public Date(string date) : this(Format(date))
        {
        }

        public Date(DateTime date)
        {
            this.Value = date;
        }

        public override string ToString()
        {
            return date.ToString("O");
        }

        /// <summary>
        /// o    2009-02-27T12:12:22.1020000-08:00
        ///      2011-07-18T10:31:11.9440212+08:00
        ///      
        /// O    2009-02-27T12:12:22.1020000-08:00
        ///      2011-07-18T10:31:11.9440212+08:00
        /// </summary>
        public static Date Now
        {
            get { return new Date(); }
        }

        /// <summary>
        /// Converts the given "OADate" (OLE Automation Date) in string format
        /// to equivalent DateTime value.
        /// </summary>
        /// <param name="oaDate"></param>
        /// <returns></returns>
        public static Date ToDate(object oaDate)
        {
            Date date = null;
            try
            {
                if (Convert.DBNull == oaDate || null == oaDate)
                {
                    date = null;
                }
                string val = Converter.ToString(oaDate);
                if (!string.IsNullOrEmpty(val) && !val.Contains("/"))
                {
                    double oaValue = double.Parse(val);
                    date = new Date(DateTime.FromOADate(oaValue));
                }
                else
                {
                    date = new Date(Convert.ToDateTime(oaDate));
                }
            }
            catch (Exception e)
            {
                Logger.WriteLog(e);
            }

            return date;
        }

        public static DateTime ToDateTime(object oaDate, DateTime def = default(DateTime))
        {
            Date date = ToDate(oaDate);
            return (null == date) ? def : date.Value;
        }

        //public static DateTime ToDateTime(DateTime date, object oaDate)
        //{
        //    Date newDate = ToDateTime(oaDate);
        //    date = (null == newDate) ? date : newDate.Value;
        //    return date;
        //}

        /// <summary>
        /// 7/1/2001 12:00:00 AM
        /// 23/07/2002 12:00:00 AM
        /// </summary>
        /// <param name="dateValue"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static DateTime Format(string dateValue, string pattern = null)
        {
            string[] formats = {
                   "M/d/yyyy h:mm:ss tt", // 7/1/2001 12:00:00 AM
                   "d/M/yyyy h:mm:ss tt", // 23/07/2002 12:00:00 AM
                   "M/d/yyyy h:mm tt", 
                   "MM/dd/yyyy hh:mm:ss", 
                   "M/d/yyyy h:mm:ss", 
                   "M/d/yyyy hh:mm tt", 
                   "M/d/yyyy hh tt", 
                   "M/d/yyyy h:mm", 
                   "M/d/yyyy h:mm", 
                   "MM/dd/yyyy hh:mm", 
                   "M/dd/yyyy hh:mm"
            };

            DateTime parsedDate;
            if (DateTime.TryParseExact(dateValue, (null == pattern) ? formats : new string[]{pattern}, null,
                                   DateTimeStyles.None, out parsedDate))
                Console.WriteLine("Converted '{0}' to {1:d}.",
                                  dateValue, parsedDate);
            else
            {
                throw new DateException("Unable to convert '{0}' to a date and time.",
                                  dateValue);
            }

            return parsedDate;
        }
    }
}
