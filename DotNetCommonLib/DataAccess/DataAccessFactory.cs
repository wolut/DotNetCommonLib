using System;
using System.Data;
using System.Configuration;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace DotNetCommonLib
{
    public static class DataAccessFactory
    {
        private static DataSourceType _dataSourceType;

        public static string ParameterFix { get; set; }
        public static string ColumnWrap { get; set; }

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

        public static IDataAccess Create()
        {
            switch (_dataSourceType)
            {
                //case DataSourceType.MySql: return new MySqlDataAccess();
                case DataSourceType.Oracle: return new OracleDataAccess();
                case DataSourceType.SqlServer: return new SqlServerDataAccess();
            }
            throw new Exception("來自DotNetCommonLib.DataAccess.DataAccessFactory的錯誤:配置文件中的數據源類型不存在或不支持！");
        }

        public static IDataAccess Create(string connectionString)
        {
            switch (_dataSourceType)
            {
                //case DataSourceType.MySql: return new MySqlDataAccess(connectionString);
                case DataSourceType.Oracle: return new OracleDataAccess(connectionString);
                case DataSourceType.SqlServer: return new SqlServerDataAccess(connectionString);
            }
            throw new Exception("來自DotNetCommonLib.DataAccess.DataAccessFactory的錯誤:配置文件中的數據源類型不存在或不支持！");
        }

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
    }
}
