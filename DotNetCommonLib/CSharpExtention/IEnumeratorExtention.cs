using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace DotNetCommonLib
{
    public static partial class CSharpExtention
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> items)
        {
            DataTable table = new DataTable();
            PropertyInfo[] ppts = items.First().GetType().GetProperties();
            foreach (PropertyInfo ppt in ppts)
            {
                table.Columns.Add(ppt.Name);
            }
            foreach (var item in items)
            {
                DataRow row = table.NewRow();
                foreach (PropertyInfo ppt in ppts)
                {
                    row[ppt.Name] = ppt.GetValue(item, null);
                }
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
