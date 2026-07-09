namespace ChipoBackend.Application.Common.Interfaces;

public interface IReportExporter
{
    byte[] ExportToCsv(ReportData data);
    byte[] ExportToExcel(ReportData data);
    byte[] ExportToPdf(ReportData data);
}

public record ReportData(
    string Title,
    string[] Headers,
    string[][] Rows,
    Dictionary<string, string>? Metadata = null
);
