using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace DotNetCommonLib
{
    /// <summary>
    /// 實體集合基類
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EntityList<T> : IEnumerable<T> where T : new()
    {
        private string _tableName;
        private List<T> _entityList = new List<T>();
        private DataTable _dataTable = null;
        protected string QueryString = string.Empty;

        public string TableName
        {
            get
            {
                if (_tableName.IsNullOrEmpty())
                    GetTableName();
                return _tableName;
            }
        }

        public int Count
        {
            get
            {
                return _entityList.Count;
            }
        }

        #region 構造函數
        /// <summary>
        /// 構造函數
        /// </summary>
        public EntityList()
        {
            string sql = string.Empty;
            if (QueryString.IsNullOrEmpty())
                sql = string.Format("SELECT * FROM {0} WHERE 1=1", TableName.ToUpper().Wrap(DataAccessFactory.ColumnWrap));
            else
                sql = QueryString;
            _dataTable = DataAccessFactory.Create().ExecuteDataTable(sql);
            DataTableToEntities(_dataTable);
        }

        public EntityList(string QueryString)
        {
            _dataTable = DataAccessFactory.Create().ExecuteDataTable(QueryString);
            DataTableToEntities(_dataTable);
        }

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="cond"></param>
        public EntityList(SqlCondition cond)
        {
            ConditionBuilder condBuilder = new ConditionBuilder();
            condBuilder.Add(cond);
            string sql = string.Format("SELECT * FROM {0} WHERE 1=1{1}", TableName.ToUpper().Wrap(DataAccessFactory.ColumnWrap), condBuilder.ConditionString);
            _dataTable = DataAccessFactory.Create().ExecuteDataTable(CommandType.Text, sql, condBuilder.ConditionParameter);
            DataTableToEntities(_dataTable);
        }

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="condBuilder"></param>
        public EntityList(ConditionBuilder condBuilder)
        {
            string sql = string.Format("SELECT * FROM {0} WHERE 1=1{1}", TableName.ToUpper().Wrap(DataAccessFactory.ColumnWrap), condBuilder.ConditionString);
            _dataTable = DataAccessFactory.Create().ExecuteDataTable(CommandType.Text, sql, condBuilder.ConditionParameter);
            DataTableToEntities(_dataTable);
        }
        #endregion

        #region 公共方法
        public DataTable ToDataTable()
        {
            return _dataTable;
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 獲取實體集合對應的數據庫表名。
        /// </summary>
        private void GetTableName()
        {
            if (_tableName.IsNullOrEmpty())
            {
                object[] tableAttribute = this.GetType().GetCustomAttributes(typeof(TableAttribute), false);
                _tableName = (tableAttribute[0] as TableAttribute).TableName;
            }
        }

        /// <summary>
        /// 將DataTable中的每一行封裝成一個實體類，並添加到
        /// </summary>
        /// <param name="dt"></param>
        private void DataTableToEntities(DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
            {
                _entityList.Add(RowToEntity(row));
            }
        }

        /// <summary>
        /// 將一行數據轉一單個實體類。
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private T RowToEntity(DataRow row)
        {
            T entity = new T();
            foreach (var item in entity.GetType().GetProperties())//獲取所有屬性列表並遍歷。
            {
                if (!row.Table.Columns.Contains(item.Name))//如果當前行內沒有該實體類型對應的屬性名，則跳過。
                    continue;
                if (item.PropertyType == typeof(string))//字符串類型
                    item.SetValue(entity, row[item.Name].ToString(), null);
                //數字類型
                else if (item.PropertyType == typeof(int) || item.PropertyType == typeof(long) || item.PropertyType == typeof(double) || item.PropertyType == typeof(decimal))
                    item.SetValue(entity, row[item.Name] ?? 0, null);
                //可空數字類型
                else if (item.PropertyType == typeof(int?) || item.PropertyType == typeof(long?) || item.PropertyType == typeof(double?) || item.PropertyType == typeof(decimal?))
                    item.SetValue(entity, row[item.Name], null);
                //布爾類型
                else if (item.PropertyType == typeof(bool))
                    item.SetValue(entity, row[item.Name] ?? false, null);
                //可空布爾類型
                else if (item.PropertyType == typeof(bool?))
                    item.SetValue(entity, row[item.Name], null);
                //時間類型
                else if (item.PropertyType == typeof(DateTime))
                {
                    DateTime outtime;
                    string datetime = row[item.Name] != null ? row[item.Name].ToString() : string.Empty;
                    DateTime.TryParse(datetime, out outtime);
                    item.SetValue(entity, outtime, null);
                }
                //可空時間類型
                else if (item.PropertyType == typeof(DateTime?))
                {
                    if (row[item.Name] == null)
                        item.SetValue(entity, null, null);
                    else
                    {
                        DateTime outtime;
                        string datetime = row[item.Name].ToString();
                        DateTime.TryParse(datetime, out outtime);
                        item.SetValue(entity, outtime, null);
                    }
                }
            }
            return entity;
        }
        #endregion

        #region 索引器
        /// <summary>
        /// 索引器(只讀)
        /// </summary>
        public T this[int index]
        {
            get
            {
                return _entityList[index];
            }
        }
        #endregion

        #region IEnumerable接口实现
        /// <summary>
        /// 显式实现IEnumerable接口
        /// </summary>
        /// <returns></returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator<T>(_entityList);
        }

        /// <summary>
        /// 不用理会此方法，之所以要实现此方法，是因为IEnumerable&lt;T&gt;接口继承了IEnumerable接口
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region 枚舉器
        /// <summary>
        /// 枚舉器类
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        class Enumerator<TItem> : IEnumerator<TItem>
        {
            private List<TItem> data = null;
            private int position = -1;

            public Enumerator(List<TItem> data)
            {
                this.data = data;
            }

            public TItem Current
            {
                get
                {
                    return data[position];
                }
            }

            public bool MoveNext()
            {
                position++;
                return position < data.Count;
            }

            public void Reset()
            {
                position = -1;
            }

            /// <summary>
            /// 不用理会此方法，之所以要实现此方法，是因为IEnumerator&lt;T&gt;接口继承了IEnumerator接口
            /// </summary>
            public void Dispose()
            {
                //throw new NotImplementedException();
            }

            /// <summary>
            /// 不用理会此方法，之所以要实现此方法，是因为IEnumerator&lt;T&gt;接口继承了IEnumerator接口
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { throw new NotImplementedException(); }
            }
        #endregion

        }
    }
}
