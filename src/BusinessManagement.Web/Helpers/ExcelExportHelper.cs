using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BusinessManagement.Web.Helpers;

public static class ExcelExportHelper
{
    public static byte[] ExportToExcel(
        string sheetName,
        List<string> headers,
        List<List<object?>> rows,
        HashSet<int>? rightAlignCols = null,
        HashSet<int>? textFormatCols = null)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add(sheetName);

        // General settings
        ws.View.ShowGridLines = true;
        ws.View.RightToLeft = true; // Right to left layout for Arabic

        // Write headers
        for (int i = 0; i < headers.Count; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
        }

        // Write rows
        for (int rowIdx = 0; rowIdx < rows.Count; rowIdx++)
        {
            var rowData = rows[rowIdx];
            for (int colIdx = 0; colIdx < headers.Count; colIdx++)
            {
                object? val = colIdx < rowData.Count ? rowData[colIdx] : null;
                
                // Format DateOnly/DateTime values nicely for Excel
                if (val is DateOnly doVal)
                {
                    ws.Cells[rowIdx + 2, colIdx + 1].Value = doVal.ToDateTime(TimeOnly.MinValue);
                    ws.Cells[rowIdx + 2, colIdx + 1].Style.Numberformat.Format = "yyyy-mm-dd";
                }
                else if (val is DateTime dtVal)
                {
                    ws.Cells[rowIdx + 2, colIdx + 1].Value = dtVal;
                    ws.Cells[rowIdx + 2, colIdx + 1].Style.Numberformat.Format = "yyyy-mm-dd hh:mm:ss";
                }
                else
                {
                    ws.Cells[rowIdx + 2, colIdx + 1].Value = val;
                }
            }
        }

        int maxRow = rows.Count + 1;
        int maxCol = headers.Count;

        // Styles
        var headerFontColor = Color.White;
        var headerBgColor = ColorTranslator.FromHtml("#1F4E78");
        var altRowBgColor = ColorTranslator.FromHtml("#F2F6F9");
        var borderColor = ColorTranslator.FromHtml("#D9D9D9");

        // Format header row (Row 1)
        ws.Row(1).Height = 28;
        using (var headerRange = ws.Cells[1, 1, 1, maxCol])
        {
            headerRange.Style.Font.Name = "Segoe UI";
            headerRange.Style.Font.Size = 11;
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.Color.SetColor(headerFontColor);
            
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(headerBgColor);
            
            headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            headerRange.Style.WrapText = true;
        }

        // Format data cells
        for (int r = 2; r <= maxRow; r++)
        {
            ws.Row(r).Height = 20;
            
            // Alternate row shading
            if (r % 2 == 0)
            {
                using (var rowRange = ws.Cells[r, 1, r, maxCol])
                {
                    rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rowRange.Style.Fill.BackgroundColor.SetColor(altRowBgColor);
                }
            }

            for (int c = 1; c <= maxCol; c++)
            {
                var cell = ws.Cells[r, c];
                cell.Style.Font.Name = "Segoe UI";
                cell.Style.Font.Size = 10;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.WrapText = true;

                // Border
                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Top.Color.SetColor(borderColor);
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Bottom.Color.SetColor(borderColor);
                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Left.Color.SetColor(borderColor);
                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                cell.Style.Border.Right.Color.SetColor(borderColor);

                // Alignment
                if (rightAlignCols != null && rightAlignCols.Contains(c))
                {
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }
                else
                {
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Text format forcing
                if (textFormatCols != null && textFormatCols.Contains(c))
                {
                    cell.Style.Numberformat.Format = "@";
                }
            }
        }

        // Border for header
        for (int c = 1; c <= maxCol; c++)
        {
            var cell = ws.Cells[1, c];
            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Top.Color.SetColor(borderColor);
            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Bottom.Color.SetColor(borderColor);
            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Left.Color.SetColor(borderColor);
            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            cell.Style.Border.Right.Color.SetColor(borderColor);
        }

        // Auto fit columns
        if (maxRow > 1)
        {
            ws.Cells[1, 1, maxRow, maxCol].AutoFitColumns(15, 40);
        }
        else
        {
            ws.Cells[1, 1, 1, maxCol].AutoFitColumns(15, 40);
        }

        return package.GetAsByteArray();
    }
}
