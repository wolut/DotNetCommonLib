using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetCommonLib
{
    /// <summary>
    /// 查詢條件對象類，表示單個查詢條件。
    /// </summary>
    public class SqlCondition
    {
        private string _express;

        /// <summary>
        /// 查詢字段名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 查詢條件表達式
        /// </summary>
        public string Express
        {
            get
            {
                if (_express == null)
                    _express = "=";
                return _express;
            }
            set
            {
                _express = value;
            }
        }

        /// <summary>
        /// 查詢條件值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 默认构造方法
        /// </summary>
        public SqlCondition()
        {

        }

        /// <summary>
        /// 構造方法
        /// </summary>
        /// <param name="Name">查詢字段名</param>
        /// <param name="Express">查詢條件表達式</param>
        /// <param name="Value">查詢條件值</param>
        public SqlCondition(string Name, string Express, object Value)
        {
            this.Name = Name;
            this.Express = Express;
            this.Value = Value;
        }
    }
}
