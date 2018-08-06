using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;
using System.Data.Common;

namespace TLib.Interfaces
{
    public interface IAttributeBase<K, V>
    {
        // Summary:
        //     Gets or sets the element with the specified key.
        //
        // Parameters:
        //   key:
        //     The key of the element to get or set.
        //
        // Returns:
        //     The element with the specified key.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        //
        //   System.NotSupportedException:
        //     The property is set and the System.Collections.IDictionary object is read-only.
        //      -or- The property is set, key does not exist in the collection, and the
        //     System.Collections.IDictionary has a fixed size.
        V this[K key] { get; set; }
    }

    public interface IAttribute : IAttributeBase<object, object>, IEnumerable
    {
        object get(object key, object defvalue = null);
        //
        // Summary:
        //     Gets an System.Collections.ICollection containing the keys in the System.Collections.Hashtable.
        //
        // Returns:
        //     An System.Collections.ICollection containing the keys in the System.Collections.Hashtable.
        ICollection Keys { get; }

        void Copy(IAttribute attrs);
    }

    //public interface IObserver<in T>
    //{
    //    /// <summary>
    //    /// Notifies the observer that the provider has finished sending push-based notifications.
    //    /// </summary>
    //    void OnCompleted();

    //    /// <summary>
    //    /// Notifies the observer that the provider has experienced an error condition.
    //    /// </summary>
    //    /// <param name="e"></param>
    //    void OnError(Exception e);

    //    /// <summary>
    //    /// Provides the observer with new data.
    //    /// </summary>
    //    /// <param name="value"></param>
    //    void OnNext(T value);

    //    void Unsubscribe();
    //}

    public interface IObservable<out T>
    {
        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        IDisposable Subscribe(IObserver<T> observer);
    }

    /// <summary>
    ///  @author Christopher Tan
    ///  @email chris.tan@beansgrp.com
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface SQLCmd<T>
    {
        //T find(object id);
        long peekResultCount(T rec);
        DataTable findAll();
        DataTable findAll(T filter);
        DataTable findAll(DataTable dt, T filter);
        T find(T rec);
        T insert(T rec);
        T update(T rec);
        T delete(T rec);
        T newObject(string prefix = null);

        //List<DataColumn> getDataColumns(string[] filters);
        //string[] filters();
        DataTable findAll(string fmt, params object[] args);
        DataTable findAll(DataTable tbl, string sql, params object[] args);

        /// <summary>
        /// To insert data into a database, use the ExecuteNonQuery method of the SqlCommand object. 
        /// The following code shows how to insert data into a database table:
        /// </summary>
        /// <param name="fmt"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        int insert(string fmt, params object[] args);

        /// <summary>
        /// Updating data
        /// The ExecuteNonQuery method is also used for updating data. 
        /// The following code shows how to update data
        /// </summary>
        /// <param name="fmt"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        int update(string fmt, params object[] args);

        object delete(string fmt, params object[] args);

        bool deleteAll(int userId);
    }

    /// <summary>
    ///  @author Christopher Tan
    ///  @email chris.tan@beansgrp.com
    /// </summary>
    public interface IDbCommand2 : IDbCommand
    {
        //
        // Summary:
        //     Adds a value to the end of the System.Data.SqlClient.SqlParameterCollection.
        //
        // Parameters:
        //   parameterName:
        //     The name of the parameter.
        //
        //   value:
        //     The value to be added.
        //
        // Returns:
        //     A System.Data.SqlClient.SqlParameter object.
        DbParameter AddWithValue(string parameterName, object value);
        object find(DataTable tbl);
        DataTable findAll(DataTable tbl);
    }

    public interface ISqlConnection
    {
        /// <summary>
        /// To insert data into a database, use the ExecuteNonQuery method of the SqlCommand object. 
        /// The following code shows how to insert data into a database table:
        /// </summary>
        /// <param name="fmt"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        int insert(string fmt, params object[] args);

        /// <summary>
        /// Updating data
        /// The ExecuteNonQuery method is also used for updating data. 
        /// The following code shows how to update data
        /// </summary>
        /// <param name="fmt"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        int update(string fmt, params object[] args);

        object delete(string fmt, params object[] args);
    }
}
