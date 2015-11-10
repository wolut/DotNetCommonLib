using System;

namespace DotNetCommonLib
{
    /// <summary>
    /// 描述Entity對象所對應的Table名稱。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// 表的名稱。
        /// </summary>
        private string _tablename;

        /// <summary>
        /// 映射為表的名稱。
        /// </summary>
        public string TableName
        {
            get { return _tablename; }
            set { _tablename = value; }
        }
    }
}
