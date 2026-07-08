// Script to extract SE weighted values totals from the Excel and verify sums
// Run from ExcelReader project directory

using ClosedXML.Excel;

var excelPath = @"c:\Users\gt114789\OneDrive - Gainwell Technologies\Projects\PROMISe Estimating Tool Migration\Promise-Estimating-Tool-POC\PROMISe_Estimating Tool.xlsm";
using var wb = new XLWorkbook(excelPath);

// The SE detail values are typically on a sheet like "Dtl SE-Development" or similar
// Let's list all sheet names first
Console.WriteLine("=== All Worksheet Names ===");
foreach (var ws in wb.Worksheets)
{
    Console.WriteLine($"  {ws.Name}");
}
