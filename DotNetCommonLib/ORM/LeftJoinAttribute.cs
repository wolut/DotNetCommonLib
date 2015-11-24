using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetCommonLib
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class LeftJoinAttribute : Attribute
    {
        private string _tableName;
        /// <summary>
        /// 要连接的表名。
        /// </summary>
        public string TableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }

        private string _alias;
        /// <summary>
        /// 串表查询时的别名。
        /// </summary>
        public string Alias
        {
            get { return _alias; }
            set { _alias = value; }
        }

        private string _joinID;
        /// <summary>
        /// 连接的ID。
        /// </summary>
        public string JoinID
        {
            get { return _joinID; }
            set { _joinID = value; }
        }

        private string _foreignKey;
        public string ForeignKey
        {
            get { return _foreignKey; }
            set { _foreignKey = value; }
        }

        private string _selectColumn;
        public string SelectColumn
        {
            get { return _selectColumn; }
            set { _selectColumn = value; }
        }

        private string _columnAlias;
        public string ColumnAlias
        {
            get { return _columnAlias; }
            set { _columnAlias = value; }
        }
    }
}
