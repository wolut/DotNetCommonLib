using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace DotNetCommonLib
{
    public static partial class CSharpExtention
    {
        /// <summary>
        /// 修改當前DataTable對象，使其僅包含該對象和指定DataTable對象的交集元素。
        /// </summary>
        /// <param name="first">被擴展的DataTable對象</param>
        /// <param name="second">需要進行交集運算的DataTable對象</param>
        public static DataTable IntersectWith(this DataTable first, DataTable second)
        {
            return first.AsEnumerable().Intersect(second.AsEnumerable(), DataRowComparer.Default).CopyToDataTable();
        }

        /// <summary>
        /// 修改當前DataTable對象，使其僅包含該對象和指定DataTable對象的差集元素。
        /// </summary>
        /// <param name="first">被擴展的DataTable對象</param>
        /// <param name="second">需要進行差集運算的DataTable對象</param>
        public static DataTable ExceptWith(this DataTable first, DataTable second)
        {
            return first.AsEnumerable().Except(second.AsEnumerable(), DataRowComparer.Default).CopyToDataTable();
        }

        /// <summary>
        /// 修改當前DataTable對象，使其包含該對象和指定DataTable對象的並集元素。
        /// </summary>
        /// <param name="first">被擴展的DataTable對象</param>
        /// <param name="second">需要進行並集運算的DataTable對象</param>
        public static DataTable UnionWith(this DataTable first, DataTable second)
        {
            return first.AsEnumerable().Union(second.AsEnumerable(), DataRowComparer.Default).CopyToDataTable();
        }

        /// <summary>
        /// 判斷DataTable中是否有重覆的指定的資料行。
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnName">要進行重覆判斷的欄位</param>
        /// <returns>如果沒有重覆的資料行則返回True，否則返回False</returns>
        public static bool IsDistinct(this DataTable table, params string[] columnName)
        {
            return table.DefaultView.ToTable(true, columnName).Rows.Count == table.Rows.Count;
        }

        /// <summary>
        /// 判斷DataTable中是否有重覆的資料行。
        /// </summary>
        /// <param name="table">被擴展的DataTable對象</param>
        /// <returns>如果沒有重覆的資料行則返回True，否則返回False</returns>
        public static bool IsDistinct(this DataTable table)
        {
            List<string> columns = new List<string>();
            foreach (DataColumn column in table.Columns)
            {
                columns.Add(column.ColumnName);
            }
            return table.DefaultView.ToTable(true, columns.ToArray()).Rows.Count == table.Rows.Count;
        }

        /// <summary>
        /// 將DataTable中的數據轉換成JSON字符串。
        /// </summary>
        /// <param name="table">被擴展的DataTable對象</param>
        /// <returns>JSON字符串</returns>
        public static string ToJSON(this DataTable table)
        {
            StringBuilder jsonBuilder = new StringBuilder();
            if (table.Rows.Count > 0)
            {
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    StringBuilder rowBuilder = new StringBuilder();
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        if (table.Rows[i][j] == DBNull.Value)
                            rowBuilder.AppendFormat("\"{0}\":null,", table.Columns[j].ColumnName);
                        else if (table.Columns[j].DataType == typeof(DateTime))
                            rowBuilder.AppendFormat("\"{0}\":\"{1}\",", table.Columns[j].ColumnName, ((DateTime)table.Rows[i][j]).ToString("yyyy/MM/dd HH:mm:ss"));
                        else if (table.Columns[j].DataType == typeof(int) || table.Columns[j].DataType == typeof(double) || table.Columns[j].DataType == typeof(decimal))
                            rowBuilder.AppendFormat("\"{0}\":{1},", table.Columns[j].ColumnName, table.Rows[i][j]);
                        else if (table.Columns[j].DataType == typeof(bool))
                            rowBuilder.AppendFormat("\"{0}\":{1},", table.Columns[j].ColumnName, (bool)table.Rows[i][j] ? "true" : "false");
                        else
                            rowBuilder.AppendFormat("\"{0}\":\"{1}\",", table.Columns[j].ColumnName, table.Rows[i][j].ToString());
                    }
                    jsonBuilder.AppendFormat("{0},", rowBuilder.ToString().TrimEnd(',').Wrap("{}"));
                }
                return jsonBuilder.ToString().TrimEnd(',').Wrap("[]");
            }
            else
            {
                return "[]";
            }
        }
    }
}
