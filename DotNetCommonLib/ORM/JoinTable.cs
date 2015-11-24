using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetCommonLib
{
    public class JoinTable
    {
        private string _tableName;
        /// <summary>
        /// 要连接的表名
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
        /// 连接的ID
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

        private List<LeftJoinAttribute> _joinColumns = new List<LeftJoinAttribute>();
        public List<LeftJoinAttribute> JoinColumns
        {
            get { return _joinColumns; }
            set { _joinColumns = value; }
        }

        public JoinTable(string tableName, string alias, string joinId, string foreignKey)
        {
            TableName = tableName;
            Alias = alias;
            JoinID = joinId;
            ForeignKey = foreignKey;
        }
    }
}
