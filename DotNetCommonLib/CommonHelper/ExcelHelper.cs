using System;
using System.IO;
using System.Data;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace DotNetCommonLib
{
    /// <summary>
    /// Excel幫助類，負責Excel的讀寫操作。
    /// </summary>
    public class ExcelHelper
    {
        /// <summary>
        /// Excel文檔類型標記
        /// </summary>
        public enum ExcelType { Excel2003, Excel2007 }

        /// <summary>
        /// 將DataSet中的數據寫入到指定的Excel文件中
        /// </summary>
        /// <param name="ds">要寫入Excel文檔中的數據</param>
        /// <param name="filePath">選擇要寫入的Excel文件格式</param>
        public static void DataSetToExcel(DataSet ds, string filePath)
        {
            string fileExt = Path.GetExtension(filePath);
            if (fileExt == ".xls")
                DataSetToExcel(ds, filePath, ExcelType.Excel2003);
            else if (fileExt == ".xlsx")
                DataSetToExcel(ds, filePath, ExcelType.Excel2007);
            else
            {
                throw new ArgumentException("來自ExcelHelper的錯誤:文件後綴名錯誤！", "filePath");
            }
        }

        /// <summary>
        /// 將DataSet中的數據寫入到指定的Excel文件中
        /// </summary>
        /// <param name="ds">要寫入Excel文檔中的數據</param>
        /// <param name="filePath">Excel文檔的完整路徑，即需要包含文件名及後綴名，後綴名只能是.xls或.xlsx</param>
        /// <param name="type">選擇要寫入的Excel文件格式</param>
        public static void DataSetToExcel(DataSet ds, string filePath, ExcelType type)
        {
            IWorkbook workbook = CreateWorkbook(type);

            for (int i = 0; i < ds.Tables.Count; i++)
            {
                DataTableToSheet(ref workbook, ds.Tables[i]);
            }

            using (FileStream fs = File.Create(filePath))
            {
                workbook.Write(fs);
            }
        }

        /// <summary>
        /// 將DataTable中的數據寫入到指定的Excel文件中
        /// </summary>
        /// <param name="dt">要寫入Excel文檔中的數據</param>
        /// <param name="filePath">Excel文檔的完整路徑，即需要包含文件名及後綴名，後綴名只能是.xls或.xlsx</param>
        public static void DataTableToExcel(DataTable dt, string filePath)
        {
            string fileExt = Path.GetExtension(filePath);
            if (fileExt == ".xls")
                DataTableToExcel(dt, filePath, ExcelType.Excel2003);
            else if (fileExt == ".xlsx")
                DataTableToExcel(dt, filePath, ExcelType.Excel2007);
            else
            {
                throw new ArgumentException("來自ExcelHelper的錯誤:文件後綴名錯誤！", "filePath");
            }
        }

        /// <summary>
        /// 將DataTable中的數據寫入到指定的Excel文件中
        /// </summary>
        /// <param name="dt">要寫入Excel文檔中的數據</param>
        /// <param name="filePath">Excel文檔的完整路徑，即需要包含文件名及後綴名，後綴名只能是.xls或.xlsx</param>
        /// <param name="type">選擇要寫入的Excel文件格式</param>
        public static void DataTableToExcel(DataTable dt, string filePath, ExcelType type)
        {
            IWorkbook workbook = CreateWorkbook(type);

            DataTableToSheet(ref workbook, dt);

            using (FileStream fs = File.Create(filePath))
            {
                workbook.Write(fs);//如果文件存在，會直接覆蓋。
            }
        }

        /// <summary>
        /// 將Excel中的數據導入DataTable中。
        /// 只會讀取第一張Sheet，且寫入DataTable中的值類型全部為String類型（因為在Excel中一列可以保存多種類型數據，DataTable做不到）
        /// 因此在回寫到數據庫時請注意轉換類型。
        /// </summary>
        /// <param name="fileName">Excle文件名</param>
        /// <param name="hasFirstColumn">第一行是否為表頭</param>
        /// <returns>DataTable</returns>
        public static DataTable ExcelToDataTable(string fileName, bool hasFirstColumn)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException(string.Format("文件不存在:{0}", fileName));

            DataTable data = new DataTable();

            using (Stream fs = File.OpenRead(fileName))
            {
                ISheet sheet = WorkbookFactory.Create(fs).GetSheetAt(0);
                data.TableName = sheet.SheetName;

                IRow firstRow = sheet.GetRow(0); //獲取第一行資料
                int columnCount = firstRow.LastCellNum; //總共的欄位數
                int rowCount = sheet.LastRowNum; //總資料行數
                if (rowCount > 0)
                {
                    //處理表頭（第一行）
                    if (hasFirstColumn) //處理有表頭標題欄的情況
                        for (int i = 0; i < columnCount; i++)
                            data.Columns.Add(new DataColumn(firstRow.GetCell(i).StringCellValue));
                    else //如無表頭，則將數據插入
                    {
                        DataRow tableRow = data.NewRow();
                        SetTableRowValue(ref tableRow, firstRow);
                        data.Rows.Add(tableRow);
                    }
                    //處理主體數據
                    for (int i = 1; i < rowCount; i++)
                    {
                        IRow sheetRow = sheet.GetRow(i);
                        DataRow tableRow = data.NewRow();
                        SetTableRowValue(ref tableRow, sheetRow);
                        data.Rows.Add(tableRow);
                    }
                }
                fs.Close();//關閉文件
            }
            return data;
        }

        /// <summary>
        /// 根據傳入的Excel類型標記，創建並返回對應的IWorkbook對象
        /// </summary>
        /// <param name="type">Excel類型標記</param>
        /// <returns>如果標記是Excel2003則返回HSSFWorkbook對象，如果標記是Excel2007則返回XSSFWorkbook對象</returns>
        private static IWorkbook CreateWorkbook(ExcelType type)
        {
            if (type == ExcelType.Excel2003)
                return new HSSFWorkbook();
            else
                return new XSSFWorkbook();
        }

        /// <summary>
        /// 在IWorkbook中新建ISheet對象，並將DataTable中的數據寫入ISheet對象中，ISheet對象的SheetName為DataTable的TableName
        /// </summary>
        /// <param name="workbook">IWorkbook對象</param>
        /// <param name="dt">要寫入的數據</param>
        private static void DataTableToSheet(ref IWorkbook workbook, DataTable dt)
        {
            string sheetName = dt.TableName.IsNullOrEmpty() ? string.Format("Sheet{0}", workbook.Count) : dt.TableName;
            ISheet sheet = workbook.CreateSheet(sheetName);
            IRow headerRow = sheet.CreateRow(0);

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string columnName = dt.Columns[i].ColumnName.IsNullOrEmpty() ? string.Format("Column{0}", i) : dt.Columns[i].ColumnName;
                headerRow.CreateCell(i).SetCellValue(columnName);
            }

            for (int j = 0; j < dt.Rows.Count; j++)
            {
                IRow eRow = sheet.CreateRow(j + 1);
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    eRow.CreateCell(i).SetCellValue(dt.Rows[j][i] != DBNull.Value ? dt.Rows[j][i].ToString() : string.Empty);
                }
            }
        }

        /// <summary>
        /// 處理一行數據
        /// </summary>
        /// <param name="tableRow">DataTable的行數據</param>
        /// <param name="sheetRow">Excel的行數據</param>
        private static void SetTableRowValue(ref DataRow tableRow, IRow sheetRow)
        {
            for (int i = 0; i < sheetRow.LastCellNum; i++)
                //因為Excel中同一列不同的行可以保存不同類型的數據，但DataTable無法做到，所以統一處理為String類型，待需要時再時行轉換。
                tableRow[i] = sheetRow.GetCell(i).ToString();
        }
    }
}