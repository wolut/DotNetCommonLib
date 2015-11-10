using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace DotNetCommonLib
{
    public static class ORMHelper
    {
        #region 公共方法
        /// <summary>
        /// 根據數據行的值自動構造實體對象
        /// </summary>
        /// <typeparam name="T">實體對象類型</typeparam>
        /// <param name="row">DataRow數據行</param>
        /// <returns>已賦值的實體對象實例</returns>
        public static T RowToEntity<T>(DataRow row) where T : new()
        {
            T Entity = new T();
            foreach (var item in typeof(T).GetProperties())//獲取所有屬性列表並遍歷。
            {
                if (!row.Table.Columns.Contains(item.Name))//如果當前行內沒有該實體類型對應的屬性名，則跳過。
                    continue;
                if (item.PropertyType == typeof(string))//字符串類型
                    item.SetValue(Entity, row[item.Name].ToString(), null);
                //數字類型
                else if (item.PropertyType == typeof(int) || item.PropertyType == typeof(long) || item.PropertyType == typeof(double) || item.PropertyType == typeof(decimal))
                    item.SetValue(Entity, row[item.Name] ?? 0, null);
                //可空數字類型
                else if (item.PropertyType == typeof(int?) || item.PropertyType == typeof(long?) || item.PropertyType == typeof(double?) || item.PropertyType == typeof(decimal?))
                    item.SetValue(Entity, row[item.Name], null);
                //布爾類型
                else if (item.PropertyType == typeof(bool))
                    item.SetValue(Entity, row[item.Name] ?? false, null);
                //可空布爾類型
                else if (item.PropertyType == typeof(bool?))
                    item.SetValue(Entity, row[item.Name], null);
                //時間類型
                else if (item.PropertyType == typeof(DateTime))
                {
                    DateTime outtime;
                    string datetime = row[item.Name] != null ? row[item.Name].ToString() : string.Empty;
                    DateTime.TryParse(datetime, out outtime);
                    item.SetValue(Entity, outtime, null);
                }
                //可空時間類型
                else if (item.PropertyType == typeof(DateTime?))
                {
                    if (row[item.Name] == null)
                        item.SetValue(Entity, null, null);
                    else
                    {
                        DateTime outtime;
                        string datetime = row[item.Name].ToString();
                        DateTime.TryParse(datetime, out outtime);
                        item.SetValue(Entity, outtime, null);
                    }
                }
            }
            return Entity;
        }

        /// <summary>
        /// 根據數據表構造實體對象集合
        /// </summary>
        /// <typeparam name="T">實體對象類型</typeparam>
        /// <param name="dataTable">DataTable數據表</param>
        /// <returns>已填充的實體對象集合</returns>
        public static List<T> DataTableToEntities<T>(DataTable dataTable) where T : new()
        {
            List<T> Entities = new List<T>(dataTable.Rows.Count);
            foreach (DataRow row in dataTable.Rows)
            {
                Entities.Add(RowToEntity<T>(row));
            }
            return Entities;
        }

        /// <summary>
        /// 根據實體對象向數據庫插入一條記錄。
        /// </summary>
        /// <typeparam name="T">實體對象類型</typeparam>
        /// <param name="entity">實體對象</param>
        /// <returns>1:成功；0:失敗。</returns>
        public static int InsertEntity<T>(T entity) where T : new()
        {
            return InsertEntity<T>(entity, null);
        }

        /// <summary>
        /// InsertEntity<T>的重載方法，根據實體對象向數據庫插入一條記錄。
        /// </summary>
        /// <typeparam name="T">實體對象類型</typeparam>
        /// <param name="entity">實體對象</param>
        /// <param name="dataAccess">IDataAccess對象</param>
        /// <returns>1:成功；0:失敗。</returns>
        public static int InsertEntity<T>(T entity, IDataAccess dataAccess) where T : new()
        {
            string sql = string.Empty;
            List<IDataParameter> parameters = GetInsertEntityParameterT_SQL(entity, out sql);
            if (dataAccess != null)
                return dataAccess.ExecuteNonQuery(CommandType.Text, sql, parameters.ToArray());
            return DataAccessFactory.Create().ExecuteNonQuery(CommandType.Text, sql, parameters.ToArray());
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static List<IDataParameter> GetInsertEntityParameterT_SQL<T>(T entity, out string sql) where T : new()
        {
            string tableName = string.Empty;
            object[] tableAttribute = typeof(T).GetCustomAttributes(typeof(DotNetCommonLib.TableAttribute), false);
            tableName = (tableAttribute[0] as DotNetCommonLib.TableAttribute).TableName;
            StringBuilder fieldBuilder = new StringBuilder();
            StringBuilder valueBuilder = new StringBuilder();
            foreach (PropertyInfo item in typeof(T).GetProperties())
            {
                object[] columnAttribute = item.GetCustomAttributes(typeof(DotNetCommonLib.ColumnAttribute), false);
                if (columnAttribute.Length > 0 && (columnAttribute[0] as DotNetCommonLib.ColumnAttribute).IsAutoIncrement)
                    continue;
                if (item.GetValue(entity, null) != null)
                {
                    fieldBuilder.AppendFormat("{0},", item.Name);
                    valueBuilder.AppendFormat("@{0},", item.Name);
                }
            }
            sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", tableName, fieldBuilder.ToString().TrimEnd(','), valueBuilder.ToString().TrimEnd(','));
            return GetActionParameter<T>(entity, TableActionType.Insert);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        private static List<IDataParameter> GetActionParameter<T>(T entity, TableActionType actionType) where T : new()
        {
            List<IDataParameter> paramList = new List<IDataParameter>();
            foreach (PropertyInfo item in typeof(T).GetProperties())
            {
                if (actionType == TableActionType.Insert)
                {
                    object[] columnAttribute = item.GetCustomAttributes(typeof(DotNetCommonLib.ColumnAttribute), false);
                    if (columnAttribute.Length > 0 && (columnAttribute[0] as DotNetCommonLib.ColumnAttribute).IsAutoIncrement)
                        continue;
                }
                paramList.Add(DataAccessFactory.CreateParameter(item.Name, item.GetValue(entity, null)));
            }
            return paramList;
        }
        #endregion
    }
}
