using ChipoBackend.Application.Common.Models;
using ChipoBackend.Application.Features.Sales.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Sales.Queries.GetSales;

public record GetSalesQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? CustomerId = null,
    DateTime? From = null,
    DateTime? To = null
) : IRequest<PagedResult<SaleListItemDto>>;

public class GetSalesQueryHandler(
    ISaleRepository saleRepository,
    ICustomerRepository customerRepository
) : IRequestHandler<GetSalesQuery, PagedResult<SaleListItemDto>>
{
    public async Task<PagedResult<SaleListItemDto>> Handle(GetSalesQuery request, CancellationToken ct)
    {
        var (sales, total) = await saleRepository.GetPagedAsync(
            request.Page, request.PageSize, request.CustomerId, request.From, request.To, ct);

        // Cache de clientes
        var customerIds = sales.Where(s => s.CustomerId.HasValue).Select(s => s.CustomerId!.Value).Distinct();
        var customers = new Dictionary<Guid, string>();
        foreach (var cid in customerIds)
        {
            var c = await customerRepository.GetByIdAsync(cid, ct);
            if (c != null) customers[cid] = c.FullName;
        }

        var items = sales.Select(s => new SaleListItemDto(
            Id: s.Id,
            SaleNumber: s.SaleNumber,
            CustomerId: s.CustomerId,
            CustomerName: s.CustomerId.HasValue ? customers.GetValueOrDefault(s.CustomerId.Value) : null,
            Channel: s.Channel.ToString(),
            ItemCount: s.Items.Count,
            Total: s.Total.Amount,
            Currency: s.Total.Currency,
            PaymentMethod: s.PaymentMethod,
            CreatedAt: s.CreatedAt
        )).ToList();

        return PagedResult<SaleListItemDto>.Create(items, total, request.Page, request.PageSize);
    }
}
