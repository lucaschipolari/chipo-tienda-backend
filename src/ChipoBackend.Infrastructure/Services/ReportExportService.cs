using System.Reflection;
using System.Text;
using ClosedXML.Excel;
using ChipoBackend.Application.Common.Interfaces;

namespace ChipoBackend.Infrastructure.Services;

public class ReportExportService : IReportExportService
{
    public byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName) where T : class
    {
        var items = data.ToList();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        using var workbook = new XLWorkbook();
        var sheetTitle = sheetName.Length > 31 ? sheetName[..31] : sheetName;
        var ws = workbook.Worksheets.Add(sheetTitle);

        // Headers
        for (int col = 0; col < properties.Length; col++)
        {
            var cell = ws.Cell(1, col + 1);
            cell.Value = properties[col].Name;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F46E5");
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Rows
        bool alternate = false;
        for (int row = 0; row < items.Count; row++)
        {
            for (int col = 0; col < properties.Length; col++)
            {
                var cell = ws.Cell(row + 2, col + 1);
                var value = properties[col].GetValue(items[row]);
                cell.Value = value switch
                {
                    null => XLCellValue.FromObject(""),
                    bool b => b,
                    int i => i,
                    long l => l,
                    decimal d => d,
                    double db => db,
                    float f => f,
                    DateTime dt => dt,
                    _ => value.ToString() ?? ""
                };
                if (alternate)
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F3F4F6");
            }
            alternate = !alternate;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportToCsv<T>(IEnumerable<T> data) where T : class
    {
        var items = data.ToList();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var sb = new StringBuilder();

        // Header row
        sb.AppendLine(string.Join(",", properties.Select(p => EscapeCsvField(p.Name))));

        // Data rows
        foreach (var item in items)
        {
            var fields = properties.Select(p =>
            {
                var val = p.GetValue(item);
                return EscapeCsvField(val?.ToString() ?? "");
            });
            sb.AppendLine(string.Join(",", fields));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] ExportToPdf(string htmlContent)
    {
        // Return the HTML as UTF-8 bytes; the browser renders / prints it as PDF.
        return Encoding.UTF8.GetBytes(htmlContent);
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
