using System;
using System.Data;
using System.Configuration;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Reflection;

namespace DotNetCommonLib
{
    /// <summary>
    /// 数据访问类工厂类
    /// </summary>
    public static class DataAccessFactory
    {
        /// <summary>
        /// 数据库类型。
        /// </summary>
        private static DataSourceType _dataSourceType;

        /// <summary>
        /// 查询参数前缀。
        /// </summary>
        public static string ParameterFix { get; set; }

        /// <summary>
        /// 关键字字段包闭字串。
        /// </summary>
        public static string ColumnWrap { get; set; }

        /// <summary>
        /// 静态构造函数。
        /// </summary>
        static DataAccessFactory()
        {
            string dataSourceType = ConfigurationManager.AppSettings["DataSourceType"] ?? "SqlServer";
            switch (dataSourceType)
            {
                case "Oracle":
                    _dataSourceType = DataSourceType.Oracle;
                    ParameterFix = ":";
                    ColumnWrap = "\"\"";
                    break;
                case "SqlServer":
                    _dataSourceType = DataSourceType.SqlServer;
                    ParameterFix = "@";
                    ColumnWrap = "[]";
                    break;
                case "MySql":
                    _dataSourceType = DataSourceType.MySql;
                    ParameterFix = "?";
                    break;
                default: throw new Exception("來自DotNetCommonLib.DataAccess.DataAccessFactory的錯誤:配置文件中的數據源類型不存在或不支持！");
            }
        }

        /// <summary>
        /// 自动创建数据库访问类。
        /// </summary>
        /// <returns></returns>
        public static IDataAccess Create()
        {
            switch (_dataSourceType)
            {
                case DataSourceType.Oracle: return new OracleDataAccess();
                case DataSourceType.SqlServer: return new SqlServerDataAccess();
            }
            throw new Exception("來自DotNetCommonLib.DataAccess.DataAccessFactory的錯誤:配置文件中的數據源類型不存在或不支持！");
        }

        /// <summary>
        /// 以连接字符串来创建数据库访问类。
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns></returns>
        public static IDataAccess Create(string connectionString)
        {
            switch (_dataSourceType)
            {
                case DataSourceType.Oracle: return new OracleDataAccess(connectionString);
                case DataSourceType.SqlServer: return new SqlServerDataAccess(connectionString);
            }
            throw new Exception("來自DotNetCommonLib.DataAccess.DataAccessFactory的錯誤:配置文件中的數據源類型不存在或不支持！");
        }

        /// <summary>
        /// 以属性名和属性值来自动创建查询参数，一般在人为指定参数时使用。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IDataParameter CreateParameter(string name, object value)
        {
            IDataParameter param;
            switch (_dataSourceType)
            {
                case DataSourceType.Oracle:
                    param = new OracleParameter(ParameterFix + name, value);
                    if (value == null)
                    {
                        ((OracleParameter)param).IsNullable = true;
                        param.Value = DBNull.Value;
                    }
                    return param;
                case DataSourceType.SqlServer:
                    param = new SqlParameter(ParameterFix + name, value);
                    if (value == null)
                    {
                        ((SqlParameter)param).IsNullable = true;
                        param.Value = DBNull.Value;
                    }
                    return param;
            }
            throw new Exception("來自DotNetCommonLib.DataAccess.DataAccessFactory的錯誤:配置文件中的數據源類型不存在或不支持！");
        }

        /// <summary>
        /// 以实体类的属性传入来建立查询参数，可以避免类型错误。
        /// </summary>
        /// <param name="Property">属性</param>
        /// <param name="Value">属性值</param>
        /// <returns></returns>
        public static IDataParameter CreateParameter(PropertyInfo Property, object Value)
        {
            IDataParameter param;
            switch (_dataSourceType)
            {
                case DataSourceType.Oracle:
                    param = new OracleParameter(ParameterFix + Property.Name, Value);
                    if (Value == null)
                    {
                        ((OracleParameter)param).IsNullable = true;
                        param.Value = DBNull.Value;
                    }
                    return param;
                case DataSourceType.SqlServer:
                    if (Property.PropertyType == typeof(Guid) || Property.PropertyType==typeof(Guid?))
                    {
                        param = new SqlParameter(ParameterFix + Property.Name, SqlDbType.UniqueIdentifier);
                        param.Value = Value;
                    }
                    else
                    {
                        param = new SqlParameter(ParameterFix + Property.Name, Value);
                        //param.Value = Value == null ? DBNull.Value : Value;
                    }
                    if (Value == null)
                    {
                        ((SqlParameter)param).IsNullable = true;
                        param.Value = DBNull.Value;
                    }
                    return param;
            }
            throw new Exception("來自DotNetCommonLib.DataAccess.DataAccessFactory的錯誤:配置文件中的數據源類型不存在或不支持！");
        }
    }
}
