using ClosedXML.Excel;

var excelPath = @"c:\Users\gt114789\OneDrive - Gainwell Technologies\Projects\PROMISe Estimating Tool Migration\Promise-Estimating-Tool-POC\PROMISe_Estimating Tool.xlsm";
using var wb = new XLWorkbook(excelPath);
var wvSheet = wb.Worksheet("DetailedEstWeightedValues");

Console.WriteLine("=== MROUND Discrepancy Analysis ===");
Console.WriteLine("Finding cases where MROUND(SUM, 0.25) differs from actual SUM\n");

// Define the component sections: (Name, firstDataRow, lastDataRow, totalRow)
var sections = new[]
{
    ("PowerBuilder Windows", 8, 15, 16),
    ("Reports", 19, 26, 27),
    ("Programs/DB Stored Procedures", 30, 37, 38),
    ("Database Manipulation", 41, 44, 45),
    ("Support Modules", 48, 51, 52),
    ("Webpage", 64, 71, 72),
    ("K2 Workflow", 75, 82, 83),
    ("K2 Smart Form", 86, 93, 94),
    ("Test Automation UFT", 98, 100, 101),
    ("MISC", 104, 108, 109),
};

// Columns: B=New Simple, C=New Moderate, D=New Complex, F=Existing Simple, G=Existing Moderate, H=Existing Complex
var colNames = new[] { ("B", "New Simple", 2), ("C", "New Moderate", 3), ("D", "New Complex", 4), 
                       ("F", "Existing Simple", 6), ("G", "Existing Moderate", 7), ("H", "Existing Complex", 8) };

int discrepancyCount = 0;

foreach (var (name, firstRow, lastRow, totalRow) in sections)
{
    foreach (var (colLetter, colName, colIndex) in colNames)
    {
        // Sum individual values
        decimal actualSum = 0;
        for (int r = firstRow; r <= lastRow; r++)
        {
            var cell = wvSheet.Cell(r, colIndex);
            var val = cell.GetString().Trim();
            if (decimal.TryParse(val, out var v))
                actualSum += v;
        }

        // Get the total row value (MROUND'd)
        var totalCell = wvSheet.Cell(totalRow, colIndex);
        var totalVal = totalCell.GetString().Trim();
        decimal excelTotal = 0;
        decimal.TryParse(totalVal, out excelTotal);

        // Compare
        if (actualSum != 0 && Math.Abs(actualSum - excelTotal) > 0.001m)
        {
            discrepancyCount++;
            Console.WriteLine($"DISCREPANCY #{discrepancyCount}: {name} | {colName}");
            Console.WriteLine($"  Actual SUM of rows {firstRow}-{lastRow}: {actualSum:N4}");
            Console.WriteLine($"  Excel MROUND total (Row {totalRow}):    {excelTotal:N4}");
            Console.WriteLine($"  Difference: {actualSum - excelTotal:N4}");
            Console.WriteLine();
        }
    }
}

if (discrepancyCount == 0)
    Console.WriteLine("No discrepancies found!");
else
    Console.WriteLine($"\nTotal discrepancies found: {discrepancyCount}");
    
Console.WriteLine("\n\nNote: Excel uses MROUND(SUM(...), 0.25) which rounds totals to nearest 0.25.");
Console.WriteLine("The POC tool uses the ACTUAL sum of individual task hours (no rounding), which is mathematically correct.");
