using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Application.Features.Reports.DTOs;
using ChipoBackend.Application.Features.Reports.Queries.GetExpensesReport;
using ChipoBackend.Application.Features.Reports.Queries.GetFinancialReport;
using ChipoBackend.Application.Features.Reports.Queries.GetInventoryReport;
using ChipoBackend.Application.Features.Reports.Queries.GetPurchasesReport;
using ChipoBackend.Application.Features.Reports.Queries.GetSalesReport;
using MediatR;

namespace ChipoBackend.Application.Features.Reports.Commands.ExportReport;

public record ExportReportCommand(ExportRequest Request) : IRequest<ExportReportResult>;

public record ExportReportResult(byte[] FileBytes, string ContentType, string FileName);

public class ExportReportCommandHandler(
    IMediator mediator,
    IReportExporter exporter
) : IRequestHandler<ExportReportCommand, ExportReportResult>
{
    public async Task<ExportReportResult> Handle(ExportReportCommand command, CancellationToken ct)
    {
        var req = command.Request;
        var from = req.From ?? DateTime.UtcNow.AddMonths(-1);
        var to = req.To ?? DateTime.UtcNow;

        var reportData = req.ReportType.ToLowerInvariant() switch
        {
            "sales" => await BuildSalesReportData(from, to, ct),
            "inventory" => await BuildInventoryReportData(req.CategoryId, req.Status, ct),
            "purchases" => await BuildPurchasesReportData(from, to, ct),
            "expenses" => await BuildExpensesReportData(from, to, req.CategoryId, req.Status, ct),
            "financial" => await BuildFinancialReportData(from, to, ct),
            _ => throw new ArgumentException($"Unknown report type: {req.ReportType}")
        };

        var format = req.Format.ToLowerInvariant();
        var (bytes, contentType) = format switch
        {
            "csv" => (exporter.ExportToCsv(reportData), "text/csv"),
            "xlsx" or "excel" => (exporter.ExportToExcel(reportData), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
            "pdf" => (exporter.ExportToPdf(reportData), "text/html"),
            _ => throw new ArgumentException($"Unknown export format: {req.Format}")
        };

        var ext = format is "xlsx" or "excel" ? "xlsx" : format == "pdf" ? "html" : "csv";
        var fileName = $"{req.ReportType}_{from:yyyyMMdd}_{to:yyyyMMdd}.{ext}";

        return new ExportReportResult(bytes, contentType, fileName);
    }


    private static readonly System.Globalization.CultureInfo ArCulture =
        System.Globalization.CultureInfo.GetCultureInfo("es-AR");

    /// <summary>300000 -> "300.000"; 1899.99 -> "1.899,99"</summary>
    private static string Fmt(decimal value) =>
        value == decimal.Truncate(value) ? value.ToString("N0", ArCulture) : value.ToString("N2", ArCulture);

    private async Task<ReportData> BuildSalesReportData(DateTime from, DateTime to, CancellationToken ct)
    {
        var report = await mediator.Send(new GetSalesReportQuery(from, to), ct);

        var headers = new[] { "Fecha", "N° Venta", "Cliente", "Canal", "Pago", "Ítems", "Subtotal", "Descuento", "Total", "Moneda" };
        var rows = report.Rows.Select(l => new[]
        {
            l.Date.ToString("yyyy-MM-dd HH:mm"),
            l.SaleNumber,
            l.BuyerName,
            l.Channel,
            l.PaymentMethod,
            l.ItemCount.ToString(),
            Fmt(l.Subtotal),
            Fmt(l.Discount),
            Fmt(l.Total),
            l.Currency
        }).ToArray();

        var meta = new Dictionary<string, string>
        {
            ["Período"] = report.Period,
            ["Total ventas"] = report.TotalCount.ToString(),
            ["Ingresos totales"] = $"{report.Currency} {Fmt(report.TotalRevenue)}",
            ["Ticket promedio"] = $"{report.Currency} {Fmt(report.AverageTicket)}"
        };

        return new ReportData("Reporte de Ventas", headers, rows, meta);
    }

    private async Task<ReportData> BuildInventoryReportData(Guid? categoryId, string? status, CancellationToken ct)
    {
        var report = await mediator.Send(new GetInventoryReportQuery(categoryId, status), ct);

        var headers = new[] { "Producto", "SKU", "Categoría", "Stock", "Precio Unitario", "Valor Stock", "Estado" };
        var rows = report.Rows.Select(l => new[]
        {
            l.ProductName,
            l.Sku,
            l.Category,
            l.CurrentStock.ToString(),
            Fmt(l.UnitPrice),
            Fmt(l.TotalValue),
            l.Status
        }).ToArray();

        var meta = new Dictionary<string, string>
        {
            ["Total productos"] = report.TotalProducts.ToString(),
            ["Sin stock"] = report.OutOfStock.ToString(),
            ["Stock crítico"] = report.Critical.ToString()
        };

        return new ReportData("Reporte de Inventario", headers, rows, meta);
    }

    private async Task<ReportData> BuildPurchasesReportData(DateTime from, DateTime to, CancellationToken ct)
    {
        var report = await mediator.Send(new GetPurchasesReportQuery(from, to), ct);

        var headers = new[] { "N° Compra", "Proveedor", "Estado", "Fecha", "Ítems", "Total", "Moneda" };
        var rows = report.Rows.Select(l => new[]
        {
            l.PurchaseNumber,
            l.SupplierName,
            l.Status,
            l.Date.ToString("yyyy-MM-dd"),
            l.ItemCount.ToString(),
            Fmt(l.Total),
            l.Currency
        }).ToArray();

        var meta = new Dictionary<string, string>
        {
            ["Costo total"] = $"{report.Currency} {Fmt(report.TotalSpent)}"
        };

        return new ReportData("Reporte de Compras", headers, rows, meta);
    }

    private async Task<ReportData> BuildExpensesReportData(DateTime from, DateTime to, Guid? categoryId, string? status, CancellationToken ct)
    {
        var report = await mediator.Send(new GetExpensesReportQuery(from, to, categoryId, status), ct);

        var headers = new[] { "Fecha", "Categoría", "Descripción", "Monto", "Moneda", "Estado" };
        var rows = report.Rows.Select(l => new[]
        {
            l.Date.ToString("yyyy-MM-dd"),
            l.Category,
            l.Description,
            Fmt(l.Amount),
            l.Currency,
            l.Status
        }).ToArray();

        var meta = new Dictionary<string, string>
        {
            ["Total gastos"] = $"{report.Currency} {Fmt(report.TotalAmount)}"
        };

        return new ReportData("Reporte de Gastos", headers, rows, meta);
    }

    private async Task<ReportData> BuildFinancialReportData(DateTime from, DateTime to, CancellationToken ct)
    {
        var report = await mediator.Send(new GetFinancialReportQuery(from, to), ct);

        var headers = new[] { "Fecha", "Ingresos", "Egresos", "Balance" };
        var rows = report.Lines.Select(l => new[]
        {
            l.Date,
            Fmt(l.Inflows),
            Fmt(l.Outflows),
            Fmt(l.Balance)
        }).ToArray();

        var meta = new Dictionary<string, string>
        {
            ["Ingresos totales"] = $"{report.Currency} {Fmt(report.TotalRevenue)}",
            ["Gastos totales"] = $"{report.Currency} {Fmt(report.TotalExpenses)}",
            ["Costo compras"] = $"{report.Currency} {Fmt(report.TotalPurchaseCosts)}",
            ["Utilidad neta"] = $"{report.Currency} {Fmt(report.NetProfit)}"
        };

        return new ReportData("Reporte Financiero", headers, rows, meta);
    }
}
