using System;
using System.Reflection;
using DotNetCommonLib;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

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
            object[] tableAttribute = this.GetType().GetCustomAttributes(typeof(TableAttribute), false);
            _tablename = (tableAttribute[0] as TableAttribute).TableName;
            //獲取屬性內容
            foreach (var item in this.GetType().GetProperties())
            {
                object[] columnAttribute = item.GetCustomAttributes(typeof(ColumnAttribute), false);
                if (columnAttribute.Length > 0)
                {
                    if ((columnAttribute[0] as ColumnAttribute).IsPrimaryKey)
                    {
                        _pkInfo = item;//獲取主鍵屬性
                    }
                    if ((columnAttribute[0] as ColumnAttribute).IsAutoIncrement)
                        _isPKAutoIncrement = true;//獲取自增長屬性
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
            string sql = string.Format("SELECT * FROM {0} WHERE {1}={2}", _tablename.Wrap(DataAccessFactory.ColumnWrap), _pkInfo.Name.Wrap(DataAccessFactory.ColumnWrap), DataAccessFactory.ParameterFix + _pkInfo.Name);
            IDataParameter param = DataAccessFactory.CreateParameter(_pkInfo.Name, id);
            _data = DataAccessFactory.Create().ExecuteDataTable(CommandType.Text, sql, param);
            if (_data.Rows.Count > 0)
                RowToEntity(_data.Rows[0]);
            else
                _data = null;
        }

        private void GetDataByCondition(ConditionBuilder condBuilder)
        {
            string sql = string.Format("SELECT * FROM {0} WHERE 1=1{1}", _tablename.ToUpper().Wrap(DataAccessFactory.ColumnWrap), condBuilder.ConditionString);
            _data = DataAccessFactory.Create().ExecuteDataTable(CommandType.Text, sql, condBuilder.ConditionParameter);
            if (_data.Rows.Count > 0)
                RowToEntity(_data.Rows[0]);
            else
                _data = null;
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
                , _tablename.ToUpper().Wrap(DataAccessFactory.ColumnWrap)
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
                StringBuilder conditionBuilder = new StringBuilder();
                conditionBuilder.AppendFormat("AND {0}={1}{2} ", _pkInfo.Name.Wrap(DataAccessFactory.ColumnWrap), DataAccessFactory.ParameterFix, _pkInfo.Name);
                StringBuilder setValueBuilder = new StringBuilder();
                List<IDataParameter> paramList = new List<IDataParameter>();
                paramList.Add(DataAccessFactory.CreateParameter(_pkInfo.Name, _pkInfo.GetValue(this, null)));//主鍵參數
                foreach (PropertyInfo item in _myProperties)
                {
                    setValueBuilder.AppendFormat("{0}={1}{2},", item.Name.Wrap(DataAccessFactory.ColumnWrap), DataAccessFactory.ParameterFix, item.Name);
                    paramList.Add(DataAccessFactory.CreateParameter(item.Name, item.GetValue(this, null)));//字段參數
                }
                string sql = string.Format("UPDATE {0} SET {1} WHERE 1=1 {2}", _tablename.Wrap(DataAccessFactory.ColumnWrap), setValueBuilder.ToString().TrimEnd(','), conditionBuilder.ToString().TrimEnd(' '));
                DataAccessFactory.Create().ExecuteNonQuery(CommandType.Text, sql, paramList.ToArray());
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
