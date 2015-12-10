#pragma warning disable 0618//消除OracleClient對象過時警告
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OracleClient;
using System.Configuration;

namespace DotNetCommonLib
{
    public class OracleDataAccess : IDataAccess, IDisposable
    {
        #region 字段
        private static string _globalConnectionString = string.Empty;
        private OracleConnection _connection;
        private OracleTransaction _transaction;
        #endregion

        #region 屬性
        /// <summary>
        /// 全局數據庫連接字符串。
        /// </summary>
        public static string GlobalConnetionString
        {
            get { return _globalConnectionString; }
        }

        /// <summary>
        /// 數據庫連接字符串。
        /// </summary>
        public string ConnectionString
        {
            get { return _connection.ConnectionString; }
            set { _connection.ConnectionString = value; }
        }

        #endregion

        #region 構造函數
        /// <summary>
        /// 靜態構造函數。
        /// </summary>
        static OracleDataAccess()
        {
            _globalConnectionString = ConfigurationManager.ConnectionStrings["System"] == null ? "" : ConfigurationManager.ConnectionStrings["System"].ConnectionString;
        }

        /// <summary>
        /// 默認實例構造函數。
        /// </summary>
        public OracleDataAccess()
        {
            if (string.IsNullOrEmpty(_globalConnectionString))
                throw new Exception("來自DotNetCommonLib.OracleDataAccess的錯誤:抓取不到配置文件中的連接字符串。");
            _connection = new OracleConnection(_globalConnectionString);
        }

        /// <summary>
        /// 重載構造函數，使用指定的連接字符串來初始化OracleDataAccess類實例。
        /// 默認所有OracleDataAccess類均使用配置文件中的ConnectionString類型的連接字符串。
        /// </summary>
        /// <param name="connectionString">數據庫連接字符串</param>
        public OracleDataAccess(string connectionString)
        {
            _connection = new OracleConnection(connectionString);
        }
        #endregion

        #region 實例方法
        /// <summary>
        /// 開啟數據庫事務。
        /// </summary>
        public void BeginTransaction()
        {
            Open();
            _transaction = _connection.BeginTransaction();
        }
        /// <summary>
        /// 提交事務處理。
        /// </summary>
        public void Commit()
        {
            _transaction.Commit();
            _transaction = null;
            Close();
        }
        /// <summary>
        /// 回滾事務處理。
        /// </summary>
        public void Rollback()
        {
            _transaction.Rollback();
            _transaction = null;
            Close();
        }

        /// <summary>
        /// 執行SQL查詢，返回受到影響的行數。
        /// </summary>
        /// <param name="cmdText">查詢命令</param>
        /// <returns>受影響的行數</returns>
        public int ExecuteNonQuery(string cmdText)
        {
            return ExecuteNonQuery(CommandType.Text, cmdText, null);
        }

        /// <summary>
        /// ExecuteNonQuery的重載方法。
        /// </summary>
        /// <param name="cmdType">查詢命令類型</param>
        /// <param name="cmdText">查詢命令</param>
        /// <param name="parameter">查詢命令參數</param>
        /// <returns>受影響的行數</returns>
        public int ExecuteNonQuery(CommandType cmdType, string cmdText, params IDataParameter[] parameter)
        {
            Open();
            OracleCommand command = InitSqlCommand(cmdType, cmdText, parameter);
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// ExecuteNonQuery的重載方法，以事務的方式執行查詢命令。
        /// </summary>
        /// <param name="transaction">OracleTransaction事務對象</param>
        /// <param name="cmdType">查詢命令類型</param>
        /// <param name="cmdText">查詢命令</param>
        /// <param name="parameter">查詢命令參數</param>
        /// <returns>受影響的行數</returns>
        public int ExecuteNonQuery(IDbTransaction transaction, CommandType cmdType, string cmdText, params IDataParameter[] parameter)
        {
            Open();
            OracleCommand command = InitSqlCommand(cmdType, cmdText, parameter);
            command.Transaction = transaction as OracleTransaction;
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// 執行查詢，返回結果集中的第一行第一列的值。
        /// </summary>
        /// <param name="cmdText">查詢命令</param>
        /// <returns>結果集中的第一行第一列的值</returns>
        public object ExecuteScalar(string cmdText)
        {
            return ExecuteScalar(CommandType.Text, cmdText, null);
        }

        /// <summary>
        /// ExecuteScalar的重載方法。
        /// </summary>
        /// <param name="cmdType">查詢命令類型</param>
        /// <param name="cmdText">查詢命令</param>
        /// <param name="parameter">查詢命令參數</param>
        /// <returns>結果集中的第一行第一列的值</returns>
        public object ExecuteScalar(CommandType cmdType, string cmdText, params IDataParameter[] parameter)
        {
            Open();
            OracleCommand command = InitSqlCommand(cmdType, cmdText, parameter);
            return command.ExecuteScalar();
        }



        /// <summary>
        /// 執行查詢命令，返回DataSet結果集。
        /// </summary>
        /// <param name="cmdText">查詢命令</param>
        /// <returns>DataSet結果集</returns>
        public DataSet ExecuteDataSet(string cmdText)
        {
            return ExecuteDataSet(CommandType.Text, cmdText, null);
        }

        /// <summary>
        /// ExecuteDataSet的重載方法。
        /// </summary>
        /// <param name="cmdType">查詢命令類型</param>
        /// <param name="cmdText">查詢命令</param>
        /// <param name="parameter">查詢命令參數</param>
        /// <returns>DataSet結果集</returns>
        public DataSet ExecuteDataSet(CommandType cmdType, string cmdText, params IDataParameter[] parameter)
        {
            Open();
            OracleCommand command = InitSqlCommand(cmdType, cmdText, parameter);
            OracleDataAdapter adapter = new OracleDataAdapter(command);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            return ds;
        }

        /// <summary>
        /// 執行查詢命令，返回DataTable結果集。
        /// </summary>
        /// <param name="cmdText">查詢命令</param>
        /// <returns>DataTable結果集</returns>
        public DataTable ExecuteDataTable(string cmdText)
        {
            return ExecuteDataTable(CommandType.Text, cmdText, null);
        }

        /// <summary>
        /// 執行查詢命令，返回DataTable結果集。
        /// </summary>
        /// <param name="cmdText">查詢命令</param>
        /// <param name="parameter">查詢命令參數</param>
        /// <returns>DataTable結果集</returns>
        public DataTable ExecuteDataTable(string cmdText, params IDataParameter[] parameter)
        {
            return ExecuteDataTable(CommandType.Text, cmdText, parameter);
        }

        /// <summary>
        /// ExecuteDataTable的重載方法。
        /// </summary>
        /// <param name="cmdType">查詢命令類型</param>
        /// <param name="cmdText">查詢命令</param>
        /// <param name="parameter">查詢命令參數</param>
        /// <returns>DataTable結果集</returns>
        public DataTable ExecuteDataTable(CommandType cmdType, string cmdText, params IDataParameter[] parameter)
        {
            Open();
            OracleCommand command = InitSqlCommand(cmdType, cmdText, parameter);
            OracleDataAdapter adapter = new OracleDataAdapter(command);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        /// <summary>
        /// 私有方法，初始化SqlCommand對象。
        /// </summary>
        /// <param name="cmdType">查詢語句類型</param>
        /// <param name="cmdText">查詢語句</param>
        /// <param name="parameter">查詢參數</param>
        /// <returns>SqlCommand對象</returns>
        public OracleCommand InitSqlCommand(CommandType cmdType, string cmdText, params IDataParameter[] parameter)
        {
            OracleCommand command = new OracleCommand(cmdText, _connection);
            command.CommandType = cmdType;
            if (_transaction != null)
                command.Transaction = _transaction;
            if (parameter != null)
                command.Parameters.AddRange(parameter);
            return command;
        }

        /// <summary>
        /// 打開數據庫連接
        /// </summary>
        public void Open()
        {
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
        }

        /// <summary>
        /// 關閉數據庫連接。
        /// </summary>
        public void Close()
        {
            if (_connection.State != ConnectionState.Closed)
                _connection.Close();
        }

        /// <summary>
        /// 執行與釋放或重置非託管資源相關的應用程序定義的任務。
        /// </summary>
        public void Dispose()
        {
            _connection.Dispose();
        }

        #endregion
    }
}
