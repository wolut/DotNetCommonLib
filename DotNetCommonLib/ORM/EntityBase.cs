using System;
using System.Reflection;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace DotNetCommonLib
{
    public abstract class EntityBase
    {
        #region 字段與屬性
        /// <summary>
        /// 主鍵屬性
        /// </summary>
        private PropertyInfo _pkInfo = null;

        /// <summary>
        /// 實體ID對應在數據庫中是否有自動增長屬性。
        /// </summary>
        private bool _isPKAutoIncrement = false;

        /// <summary>
        /// 除主鍵屬性以外的其他屬性集合
        /// </summary>
        private List<PropertyInfo> _myProperties = new List<PropertyInfo>();

        /// <summary>
        /// 與實體類綁定的數據庫表名。
        /// </summary>
        private string _tablename = string.Empty;

        /// <summary>
        /// 执行串表查询时表示的别名。
        /// </summary>
        private string _aliasName = string.Empty;

        /// <summary>
        /// 標記實體類當前的狀態是否為刪除。
        /// </summary>
        private bool Deleted = false;

        /// <summary>
        /// 標記實體類是否存在於數據庫。
        /// </summary>
        protected bool Inserted = false;

        /// <summary>
        /// 標記此對象資源是否已被手動釋放。
        /// </summary>
        private bool Disposed = false;

        /// <summary>
        /// 標記此對象是否已被初始化。
        /// </summary>
        private bool Initialized = false;

        /// <summary>
        /// 用於保存從DB取得的數據。
        /// </summary>
        private DataTable _data = null;

        private Dictionary<string, JoinTable> _joinTables = new Dictionary<string, JoinTable>();
        #endregion

        #region 構造方法
        /// <summary>
        /// 默認構造器。
        /// </summary>
        public EntityBase()
        {
            GetBindingInfo();
        }

        /// <summary>
        /// 針對整型ID的構造器。
        /// </summary>
        /// <param name="id"></param>
        public EntityBase(int id)
            : this()
        {
            Inserted = true;
            GetDataById<int>(id);
        }

        /// <summary>
        /// 針對字符串類型ID的構造器。
        /// </summary>
        /// <param name="id"></param>
        public EntityBase(string id)
            : this()
        {
            Inserted = true;
            GetDataById<string>(id);
        }

        public EntityBase(Guid id)
            : this()
        {
            Inserted = true;
            GetDataById<Guid>(id);
        }

        public EntityBase(ConditionBuilder condBuilder)
            : this()
        {
            Inserted = true;
            GetDataByCondition(condBuilder);
        }
        #endregion

        /// <summary>
        /// 獲取與該實體綁定的表信息——表名和主鍵。
        /// </summary>
        protected void GetBindingInfo()
        {
            //獲取關聯表名
            TableAttribute tableAttr = this.GetType().GetCustomAttributes(typeof(TableAttribute), false)[0] as TableAttribute;
            _tablename = tableAttr.TableName.ToUpper().Wrap(DataAccessFactory.ColumnWrap);
            _aliasName = tableAttr.AliasName;
            //獲取屬性內容
            foreach (var item in this.GetType().GetProperties())
            {
                object[] columnAttribute = item.GetCustomAttributes(typeof(ColumnAttribute), false);
                object[] joinAttrs = item.GetCustomAttributes(typeof(LeftJoinAttribute), false);
                if (columnAttribute.Length > 0)
                {
                    ColumnAttribute colAttr = (columnAttribute[0] as ColumnAttribute);
                    if (colAttr.IsPrimaryKey)
                    {
                        _pkInfo = item;//獲取主鍵屬性
                    }
                }
                else if (joinAttrs.Length > 0)//收集串表栏位
                {
                    LeftJoinAttribute joinAttr = (joinAttrs[0] as LeftJoinAttribute);
                    joinAttr.ColumnAlias = item.Name;
                    if (!_joinTables.ContainsKey(joinAttr.Alias))
                    {
                        JoinTable jt = new JoinTable(joinAttr.TableName, joinAttr.Alias, joinAttr.JoinID, joinAttr.ForeignKey);
                        jt.JoinColumns.Add(joinAttr);
                        _joinTables.Add(joinAttr.Alias, jt);
                    }
                    else
                    {
                        _joinTables[joinAttr.Alias].JoinColumns.Add(joinAttr);
                    }
                }
                else
                {
                    _myProperties.Add(item);//獲取其他屬性
                }
            }
        }

        /// <summary>
        /// 根據傳入的ID信息，從數據庫對應的表中抓取一條數據，並自己填充欄位內容到對應屬性。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">表的ID值</param>
        private void GetDataById<T>(T id)
        {
            string sql = string.Format("SELECT {0} {1} {2}", GetSelectColumnString(), GetSelectFromString(), GetSelectWhereStringById());
            IDataParameter param = DataAccessFactory.CreateParameter(_pkInfo, id);
            _data = DataAccessFactory.Create().ExecuteDataTable(CommandType.Text, sql, param);
            if (_data.Rows.Count > 0)
                RowToEntity(_data.Rows[0]);
            else
                _data = null;
        }

        private void GetDataByCondition(ConditionBuilder condBuilder)
        {
            //TODO:这里也是一样的，需要重新组装连接表的SQL语句，设想再开一个私有方法来处理。2015/11/21
            string sql = string.Format("SELECT {0} {1} WHERE 1=1 {2}", GetSelectColumnString(), GetSelectFromString(), condBuilder.ConditionString);
            _data = DataAccessFactory.Create().ExecuteDataTable(CommandType.Text, sql, condBuilder.ConditionParameter);
            if (_data.Rows.Count > 0)
                RowToEntity(_data.Rows[0]);
            else
                _data = null;
        }

        /// <summary>
        /// 组装SQL语句的查询栏位部分。
        /// </summary>
        private string GetSelectColumnString()
        {
            string columnStr=_pkInfo.Name.Wrap(DataAccessFactory.ColumnWrap);
            if (!_aliasName.IsNullOrEmpty())
                columnStr = _aliasName.Wrap(DataAccessFactory.ColumnWrap) + "." + columnStr;
            foreach (var item in _myProperties)
            {
                columnStr += "," + item.Name;
            }
            foreach (var join in _joinTables.Values)
            {
                foreach (var attr in join.JoinColumns)
                {
                    columnStr += string.Format(",{0}.{1} AS {2}", attr.Alias.Wrap(DataAccessFactory.ColumnWrap), attr.SelectColumn.Wrap(DataAccessFactory.ColumnWrap), attr.ColumnAlias);
                }
            }
            return columnStr;
        }

        /// <summary>
        /// 组装SQL语句的串表部分。
        /// </summary>
        /// <returns></returns>
        private string GetSelectFromString()
        {
            string fromStr = string.Format("FROM {0}", _tablename);
            if (_joinTables.Count == 0)
            {
                return fromStr;
            }
            else
            {
                if(!_aliasName.IsNullOrEmpty())
                    fromStr += " AS " + _aliasName;
                foreach (var item in _joinTables.Values)
                {
                    fromStr += string.Format(" LEFT JOIN {0} AS {1} ON {1}.{2}={3}.{4}"
                        , item.TableName.Wrap(DataAccessFactory.ColumnWrap)
                        , item.Alias
                        , item.JoinID.Wrap(DataAccessFactory.ColumnWrap)
                        , _aliasName
                        , item.ForeignKey);
                }
                return fromStr;
            }
        }

        /// <summary>
        /// 组装SQL语句的Where条件部分
        /// </summary>
        /// <returns></returns>
        private string GetSelectWhereStringById()
        {
            string whereStr = "WHERE 1=1";
            if (_joinTables.Count == 0)
            {
                whereStr += string.Format(" AND {0}={1}", _pkInfo.Name.Wrap(DataAccessFactory.ColumnWrap), DataAccessFactory.ParameterFix + _pkInfo.Name);
            }
            else
            {
                whereStr += string.Format(" AND {0}.{1}={2}", _aliasName, _pkInfo.Name.Wrap(DataAccessFactory.ColumnWrap), DataAccessFactory.ParameterFix + _pkInfo.Name);
            }
            return whereStr;
        }

        /// <summary>
        /// 將從數據庫中取得的資料根據類型自動直充到對應的屬性中。
        /// </summary>
        /// <param name="row">數據庫中抓取的一行數據</param>
        private void RowToEntity(DataRow row)
        {
            foreach (var item in this.GetType().GetProperties())//獲取所有屬性列表並遍歷。
            {
                if (!row.Table.Columns.Contains(item.Name))//如果當前行內沒有該實體類型對應的屬性名，則跳過。
                    continue;
                if (item.PropertyType == typeof(string))//字符串類型
                    item.SetValue(this, row[item.Name].ToString(), null);
                //數字類型
                else if (item.PropertyType == typeof(int) || item.PropertyType == typeof(long) || item.PropertyType == typeof(double) || item.PropertyType == typeof(decimal))
                    item.SetValue(this, row[item.Name] ?? 0, null);
                //可空數字類型
                else if (item.PropertyType == typeof(int?) || item.PropertyType == typeof(long?) || item.PropertyType == typeof(double?) || item.PropertyType == typeof(decimal?))
                    item.SetValue(this, row[item.Name] == DBNull.Value ? null : row[item.Name], null);
                //布爾類型
                else if (item.PropertyType == typeof(bool))
                    item.SetValue(this, row[item.Name] ?? false, null);
                //可空布爾類型
                else if (item.PropertyType == typeof(bool?))
                    item.SetValue(this, row[item.Name], null);
                //Guid
                else if (item.PropertyType == typeof(Guid))
                    item.SetValue(this, row[item.Name], null);
                //時間類型
                else if (item.PropertyType == typeof(DateTime))
                {
                    DateTime outtime;
                    string datetime = row[item.Name] != null ? row[item.Name].ToString() : string.Empty;
                    DateTime.TryParse(datetime, out outtime);
                    item.SetValue(this, outtime, null);
                }
                //可空時間類型
                else if (item.PropertyType == typeof(DateTime?))
                {
                    if (row[item.Name] == null)
                        item.SetValue(this, null, null);
                    else
                    {
                        DateTime outtime;
                        string datetime = row[item.Name].ToString();
                        DateTime.TryParse(datetime, out outtime);
                        item.SetValue(this, outtime, null);
                    }
                }
            }
        }

        #region 公共方法
        /// <summary>
        /// 將對象內容保存至數據庫中。
        /// </summary>
        public void Save()
        {
            StringBuilder fieldBuilder = new StringBuilder();
            StringBuilder valueBuilder = new StringBuilder();
            List<IDataParameter> paramList = new List<IDataParameter>();
            //如果主鍵不是自動增長的，那就要組裝在SQL語句中，同時也要生成查詢參數。
            if (!_isPKAutoIncrement)
            {
                fieldBuilder.AppendFormat("{0},", _pkInfo.Name.ToUpper().Wrap(DataAccessFactory.ColumnWrap));
                valueBuilder.AppendFormat("{0}{1},", DataAccessFactory.ParameterFix, _pkInfo.Name.ToUpper());
                paramList.Add(DataAccessFactory.CreateParameter(_pkInfo.Name, _pkInfo.GetValue(this, null)));
            }
            foreach (PropertyInfo item in _myProperties)
            {
                fieldBuilder.AppendFormat("{0},", item.Name.ToUpper().Wrap(DataAccessFactory.ColumnWrap));
                valueBuilder.AppendFormat("{0}{1},", DataAccessFactory.ParameterFix, item.Name.ToUpper());
                paramList.Add(DataAccessFactory.CreateParameter(item.Name.ToUpper(), item.GetValue(this, null)));
            }
            //組裝SQL語句
            string sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})"
                , _tablename
                , fieldBuilder.ToString().TrimEnd(',')
                , valueBuilder.ToString().TrimEnd(',')
            );
            //執行查詢語句
            DataAccessFactory.Create().ExecuteNonQuery(CommandType.Text, sql, paramList.ToArray());
            //標記已插入DB
            Inserted = true;
        }

        /// <summary>
        /// 從數據庫中刪除此實體類對應的數據記錄。
        /// </summary>
        /// <typeparam name="T">泛型類型</typeparam>
        /// <param name="id">數據記錄的ID，一般是數據或字符串類型</param>
        public void Delete()
        {
            if (!Inserted)
            {
                throw new Exception("來自EntityBase.Delete()的錯誤:此數據還未插入數據庫!");
            }
            else if (_pkInfo == null)
            {
                throw new Exception("來自EntityBase.Delete()的錯誤:主鍵不能為空！");
            }
            else if (_pkInfo.GetValue(this, null).ToString().IsNullOrEmpty())
            {
                throw new Exception("來自EntityBase.Delete()的錯誤:主鍵值不能為空！");
            }
            else if (Deleted)
            {
                throw new Exception("來自EntityBase.Delete()的錯誤:此對象已經在數據庫中被刪除，請勿重覆操作！");
            }
            else
            {
                string sql = string.Format("DELETE {0} WHERE {1}={2}", _tablename, _pkInfo.Name, DataAccessFactory.ParameterFix + _pkInfo.Name);
                IDataParameter param = DataAccessFactory.CreateParameter(_pkInfo.Name, _pkInfo.GetValue(this, null));
                if (DataAccessFactory.Create().ExecuteNonQuery(CommandType.Text, sql, param) > 0)
                {
                    Deleted = true;//標記為已刪除
                    Dispose();//手動釋放對象資源
                }
                else
                {
                    throw new Exception("來自EntityBase.Delete()的錯誤:此對象不存在於數據庫中！");
                }
            }
        }

        /// <summary>
        /// 如果對象屬性值發生變化，則會更新到數據庫中。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        public void Update()
        {
            if (_pkInfo == null)
            {
                throw new Exception("來自EntityBase.Update()的錯誤:沒有主鍵信息，不能執行更新操作！");
            }
            else if (_pkInfo.GetValue(this, null).ToString().IsNullOrEmpty())
            {
                throw new Exception("來自EntityBase.Update()的錯誤:主鍵值不能為空！");
            }
            else
            {
                try
                {
                    StringBuilder conditionBuilder = new StringBuilder();
                    conditionBuilder.AppendFormat("AND {0}={1}{2} ", _pkInfo.Name.Wrap(DataAccessFactory.ColumnWrap), DataAccessFactory.ParameterFix, _pkInfo.Name);
                    StringBuilder setValueBuilder = new StringBuilder();
                    List<IDataParameter> paramList = new List<IDataParameter>();
                    paramList.Add(DataAccessFactory.CreateParameter(_pkInfo, _pkInfo.GetValue(this, null)));//主鍵參數
                    foreach (PropertyInfo item in _myProperties)
                    {
                        setValueBuilder.AppendFormat("{0}={1}{2},", item.Name.Wrap(DataAccessFactory.ColumnWrap), DataAccessFactory.ParameterFix, item.Name);
                        paramList.Add(DataAccessFactory.CreateParameter(item, item.GetValue(this, null)));//字段參數
                    }
                    string sql = string.Format("UPDATE {0} SET {1} WHERE 1=1 {2}", _tablename, setValueBuilder.ToString().TrimEnd(','), conditionBuilder.ToString().TrimEnd(' '));
                    DataAccessFactory.Create().ExecuteNonQuery(CommandType.Text, sql, paramList.ToArray());
                }
                catch (Exception ex)
                {
                    throw new Exception("來自EntityBase.Update()的錯誤:" + ex.Message);
                }
            }
        }

        public bool Exists()
        {
            if (_data != null && _data.Rows.Count > 0)
                return true;
            return false;
        }

        public DataTable ToDataTable()
        {
            return _data;
        }

        #endregion

        /// <summary>
        /// 刪除實體時，釋放此對象資源。
        /// </summary>
        private void Dispose()
        {
            if (Disposed)
                throw new Exception("來自EntityBase.Dispose()的錯誤:此對象資源已被釋放！");
            foreach (PropertyInfo item in this.GetType().GetProperties())
            {
                item.SetValue(this, null, null);
            }
            Disposed = true;
        }
    }
}
