using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace DotNetCommonLib
{
    /// <summary>
    /// SQL查詢條件語句拼裝功能類。
    /// </summary>
    public class ConditionBuilder
    {
        /// <summary>
        /// 條件對象存放容器。
        /// </summary>
        private List<SqlCondition> conditions = new List<SqlCondition>();

        /// <summary>
        /// 獲取對應的SQL查詢條件語句。
        /// </summary>
        public string ConditionString
        {
            get
            {
                return ToCommandText();
            }
        }

        /// <summary>
        /// 獲取對應的SQL查詢參數對象數組。
        /// </summary>
        public IDataParameter[] ConditionParameter
        {
            get
            {
                return ToCommandParameter();
            }
        }

        /// <summary>
        /// 構造方法。
        /// </summary>
        public ConditionBuilder()
        {

        }

        /// <summary>
        /// 往對象中添加查詢條件對象(SqlCondition)。
        /// </summary>
        /// <param name="Name">查詢條件字段</param>
        /// <param name="Express">查詢表達式</param>
        /// <param name="Value">查詢條件值</param>
        public void Add(string Name, string Express, object Value)
        {
            SqlCondition cond = new SqlCondition(Name, Express, Value);
            conditions.Add(cond);
        }

        /// <summary>
        /// 往對象中添加查詢條件對象(SqlCondition)。
        /// </summary>
        /// <param name="cond">SqlCondition對象</param>
        public void Add(SqlCondition cond)
        {
            conditions.Add(cond);
        }

        /// <summary>
        /// 往對象中添加查詢條件對象(SqlCondition)。
        /// </summary>
        /// <param name="conds">SqlCondition對象數組</param>
        public void Add(SqlCondition[] conds)
        {
            foreach (SqlCondition cond in conds)
            {
                conditions.Add(cond);
            }
        }

        /// <summary>
        /// 清空所有已經添加的查詢條件對象
        /// </summary>
        public void Clear()
        {
            conditions.Clear();
        }

        /// <summary>
        /// 獲取對應的SQL查詢條件語句。
        /// </summary>
        /// <returns>SQL查詢條件語句</returns>
        private string ToCommandText()
        {
            StringBuilder conditionBuilder = new StringBuilder();
            if (conditions.Count > 0)
            {
                foreach (SqlCondition cond in conditions)
                {
                    if (cond.Express.ToUpper().Trim() == "NULL" || cond.Express.ToUpper().Trim() == "IS NULL")
                        conditionBuilder.AppendFormat(" AND {0} IS NULL", cond.Name.ToUpper().Wrap(DataAccessFactory.ColumnWrap));
                    else if (cond.Express.ToUpper().Trim() == "!NULL" || cond.Express.ToUpper().Trim() == "IS NOT NULL")
                        conditionBuilder.AppendFormat(" AND {0} IS NOT NULL", cond.Name.ToUpper().Wrap(DataAccessFactory.ColumnWrap));
                    else if (cond.Express.ToUpper().Trim() == "LIKE" || cond.Express.ToUpper().Trim() == "NOT LIKE")
                    {
                        // TODO 涉及一些通配符的處理問題
                        //conditionBuilder.AppendFormat(" AND {0} {1} {2}{3}"
                        //    , cond.Name.ToUpper().Wrap(DataAccessFactory.ColumnWrap)
                        //    , cond.Express
                        //    , DataAccessFactory.ParameterFix
                        //    , cond.Name.ToUpper());
                    }
                    else
                        conditionBuilder.AppendFormat(" AND {0} {1} {2}{3}"
                            , cond.Name.ToUpper().Wrap(DataAccessFactory.ColumnWrap)
                            , cond.Express
                            , DataAccessFactory.ParameterFix
                            , cond.Name.ToUpper());
                }
            }
            return conditionBuilder.ToString();
        }

        /// <summary>
        /// 獲取對應的SQL查詢參數對象數組。
        /// </summary>
        /// <returns>SQL查詢參數對象數組</returns>
        private IDataParameter[] ToCommandParameter()
        {
            List<IDataParameter> paramList = new List<IDataParameter>();
            if (conditions.Count > 0)
            {
                foreach (SqlCondition cond in conditions)
                {
                    paramList.Add(DataAccessFactory.CreateParameter(cond.Name, cond.Value));
                }
            }
            return paramList.ToArray();
        }
    }
}
