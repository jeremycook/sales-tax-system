using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;

namespace SiteKit.NPOI
{
    public static class WorkbookHelpers
    {
        /// <summary>
        /// Creates an empty <see cref="IWorkbook"/>.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static IWorkbook CreateWorkbook(string extension = ".xlsx")
        {
            IWorkbook workbook;
            if (extension.ToLower() == ".xlsx")
            {
                workbook = new XSSFWorkbook();
            }
            else if (extension.ToLower() == ".xls")
            {
                workbook = new HSSFWorkbook();
            }
            else
            {
                throw new ArgumentException($"The {extension} file extension is not supported. Only .xlsx and .xls are.", nameof(extension));
            }

            return workbook;
        }

        /// <summary>
        /// Creates an <see cref="IWorkbook"/> filled with data from <paramref name="dataTable"/>.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="extension"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public static IWorkbook CreateWorkbook(System.Data.DataTable dataTable, string extension = ".xlsx", string sheetName = "Sheet 1")
        {
            IWorkbook workbook = CreateWorkbook(extension);
            CreateSheet(workbook, sheetName, dataTable);
            return workbook;
        }

        /// <summary>
        /// Creates an <see cref="IWorkbook"/> filled with data from <paramref name="list"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="extension"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public static IWorkbook CreateWorkbook<T>(IEnumerable<T> list, string extension = ".xlsx", string sheetName = "Sheet 1")
        {
            var dataTable = list.ToDataTable();
            var workbook = CreateWorkbook(dataTable, extension, sheetName);
            return workbook;
        }

        /// <summary>
        /// Creates an <see cref="ISheet"/> in the workbook, fills it based on the <paramref name="dataTable"/>, and returns it.
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="sheetName"></param>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static ISheet CreateSheet(IWorkbook workbook, string sheetName, System.Data.DataTable dataTable)
        {
            var sheet = workbook.CreateSheet(sheetName);
            var headerRow = sheet.CreateRow(0);
            var dataFormat = workbook.CreateDataFormat();

            for (int j = 0; j < dataTable.Columns.Count; j++)
            {
                var cell = headerRow.CreateCell(j);
                var dataCol = dataTable.Columns[j];

                cell.SetCellValue(dataCol.ColumnName);

                if (typeof(DateTime?).IsAssignableFrom(dataCol.DataType))
                {
                    var cellStyle = workbook.CreateCellStyle();
                    cellStyle.DataFormat = dataFormat.GetFormat("MM/dd/yyyy");
                    sheet.SetDefaultColumnStyle(j, cellStyle);
                }
            }

            // Fill in the data
            for (int iRow = 0; iRow < dataTable.Rows.Count; iRow++)
            {
                var row = sheet.CreateRow(iRow + 1);

                for (int iCol = 0; iCol < dataTable.Columns.Count; iCol++)
                {
                    var col = dataTable.Columns[iCol];
                    object value = dataTable.Rows[iRow][col.ColumnName];

                    if (value == null || value == DBNull.Value)
                    {
                        // Skip
                    }
                    else
                    {
                        var cell = row.CreateCell(iCol);
                        switch (value)
                        {
                            case bool val:
                                cell.SetCellValue(val);
                                break;
                            case float val:
                                cell.SetCellValue(val);
                                break;
                            case double val:
                                cell.SetCellValue(val);
                                break;
                            case decimal val:
                                cell.SetCellValue((double)val);
                                break;
                            case DateTime val:
                                cell.SetCellValue(val);
                                break;
                            default:
                                cell.SetCellValue(value.ToString());
                                break;
                        }
                    }
                }
            }

            // Autosize after the data is populated
            const int maxWidth = 256 * 20;
            for (int iCol = 0; iCol < headerRow.LastCellNum; iCol++)
            {
                sheet.AutoSizeColumn(iCol);
                if (sheet.GetColumnWidth(iCol) > maxWidth)
                {
                    sheet.SetColumnWidth(iCol, maxWidth);
                }
            }

            return sheet;
        }
    }
}
