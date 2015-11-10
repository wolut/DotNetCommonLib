using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace DotNetCommonLib
{
    /// <summary>
    /// 通用數據源訪問接口
    /// </summary>
    public interface IDataAccess
    {
        string ConnectionString { get; set; }

        void BeginTransaction();

        void Commit();

        void Rollback();

        int ExecuteNonQuery(string cmdText);

        int ExecuteNonQuery(CommandType cmdType, string cmdText, params IDataParameter[] parameter);

        int ExecuteNonQuery(IDbTransaction trans, CommandType cmdType, string cmdText, params IDataParameter[] parameter);

        object ExecuteScalar(string cmdText);

        object ExecuteScalar(CommandType cmdType, string cmdText, params IDataParameter[] parameter);

        DataSet ExecuteDataSet(string cmdText);

        DataSet ExecuteDataSet(CommandType cmdType, string cmdText, params IDataParameter[] parameter);

        DataTable ExecuteDataTable(string cmdText);

        DataTable ExecuteDataTable(CommandType cmdType, string cmdText, params IDataParameter[] parameter);

        DataTable ExecuteDataTable(string cmdText, params IDataParameter[] parameter);

        void Open();

        void Close();

        void Dispose();
    }
}
