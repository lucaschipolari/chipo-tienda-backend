namespace ChipoBackend.Application.Common.Interfaces;

public interface IReportExportService
{
    byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName) where T : class;
    byte[] ExportToCsv<T>(IEnumerable<T> data) where T : class;
    byte[] ExportToPdf(string htmlContent);
}
