using System.Text;
using ClosedXML.Excel;
using ChipoBackend.Application.Common.Interfaces;

namespace ChipoBackend.Infrastructure.Services;

public class ReportExporter : IReportExporter
{
    public byte[] ExportToCsv(ReportData data)
    {
        var sb = new StringBuilder();

        // Metadata block
        if (data.Metadata is { Count: > 0 })
        {
            foreach (var (key, value) in data.Metadata)
                sb.AppendLine($"{EscapeCsvField(key)},{EscapeCsvField(value)}");
            sb.AppendLine();
        }

        // Headers
        sb.AppendLine(string.Join(",", data.Headers.Select(EscapeCsvField)));

        // Rows
        foreach (var row in data.Rows)
            sb.AppendLine(string.Join(",", row.Select(EscapeCsvField)));

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] ExportToExcel(ReportData data)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(data.Title.Length > 31 ? data.Title[..31] : data.Title);

        int currentRow = 1;

        // Title
        ws.Cell(currentRow, 1).Value = data.Title;
        ws.Cell(currentRow, 1).Style.Font.Bold = true;
        ws.Cell(currentRow, 1).Style.Font.FontSize = 14;
        currentRow += 2;

        // Metadata
        if (data.Metadata is { Count: > 0 })
        {
            foreach (var (key, value) in data.Metadata)
            {
                ws.Cell(currentRow, 1).Value = key;
                ws.Cell(currentRow, 1).Style.Font.Bold = true;
                ws.Cell(currentRow, 2).Value = value;
                currentRow++;
            }
            currentRow++;
        }

        // Headers
        for (int col = 0; col < data.Headers.Length; col++)
        {
            var cell = ws.Cell(currentRow, col + 1);
            cell.Value = data.Headers[col];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F46E5");
            cell.Style.Font.FontColor = XLColor.White;
        }
        currentRow++;

        // Rows
        bool alternate = false;
        foreach (var row in data.Rows)
        {
            for (int col = 0; col < row.Length; col++)
            {
                var cell = ws.Cell(currentRow, col + 1);
                cell.Value = row[col];
                if (alternate)
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F3F4F6");
            }
            alternate = !alternate;
            currentRow++;
        }

        // Auto-fit columns
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportToPdf(ReportData data)
    {
        // Render an HTML table suitable for browser print-to-PDF
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"es\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"UTF-8\">");
        sb.AppendLine($"<title>{HtmlEncode(data.Title)}</title>");
        sb.AppendLine("""
<style>
  body { font-family: Arial, sans-serif; font-size: 12px; margin: 20px; color: #111; }
  h1 { font-size: 18px; margin-bottom: 8px; }
  .meta { margin-bottom: 16px; }
  .meta span { display: inline-block; margin-right: 24px; }
  table { border-collapse: collapse; width: 100%; }
  th { background: #4F46E5; color: #fff; padding: 6px 10px; text-align: left; }
  td { padding: 5px 10px; border-bottom: 1px solid #e5e7eb; }
  tr:nth-child(even) td { background: #f3f4f6; }
  @media print { body { margin: 0; } }
</style>
""");
        sb.AppendLine("</head><body>");
        sb.AppendLine($"<h1>{HtmlEncode(data.Title)}</h1>");

        if (data.Metadata is { Count: > 0 })
        {
            sb.AppendLine("<div class=\"meta\">");
            foreach (var (key, value) in data.Metadata)
                sb.AppendLine($"<span><strong>{HtmlEncode(key)}:</strong> {HtmlEncode(value)}</span>");
            sb.AppendLine("</div>");
        }

        sb.AppendLine("<table>");
        sb.AppendLine("<thead><tr>");
        foreach (var h in data.Headers)
            sb.AppendLine($"<th>{HtmlEncode(h)}</th>");
        sb.AppendLine("</tr></thead>");
        sb.AppendLine("<tbody>");
        foreach (var row in data.Rows)
        {
            sb.AppendLine("<tr>");
            foreach (var cell in row)
                sb.AppendLine($"<td>{HtmlEncode(cell)}</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</tbody></table>");
        sb.AppendLine("</body></html>");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    private static string HtmlEncode(string text) =>
        text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
}
