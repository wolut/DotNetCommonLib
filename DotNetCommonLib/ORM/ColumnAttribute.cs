using System;

namespace DotNetCommonLib
{
    /// <summary>
    /// 描述欄位對就的屬性，如主鍵，自動增長等信息。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// 描述欄位是否為表主鍵。
        /// </summary>
        private bool _isPrimaryKey;
        public bool IsPrimaryKey
        {
            get { return _isPrimaryKey; }
            set { _isPrimaryKey = value; }
        }

        /// <summary>
        /// 描述欄位是否為自增長欄位。
        /// </summary>
        private bool _isAutoIncrement;
        public bool IsAutoIncrement
        {
            get { return _isAutoIncrement; }
            set { _isAutoIncrement = value; }
        }
    }
}
