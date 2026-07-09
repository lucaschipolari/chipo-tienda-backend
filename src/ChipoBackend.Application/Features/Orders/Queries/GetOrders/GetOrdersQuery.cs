using ChipoBackend.Application.Common.Models;
using ChipoBackend.Application.Features.Orders.DTOs;
using ChipoBackend.Domain.Entities.Orders;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Orders.Queries.GetOrders;

public record GetOrdersQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? CustomerId = null,
    string? Status = null,
    DateTime? From = null,
    DateTime? To = null,
    string? Search = null,
    string? Email = null
) : IRequest<PagedResult<OrderListItemDto>>;

public class GetOrdersQueryHandler(
    IOrderRepository orderRepository,
    ICustomerRepository customerRepository
) : IRequestHandler<GetOrdersQuery, PagedResult<OrderListItemDto>>
{
    public async Task<PagedResult<OrderListItemDto>> Handle(GetOrdersQuery request, CancellationToken ct)
    {
        OrderStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<OrderStatus>(request.Status, true, out var s))
            status = s;

        var (orders, total) = await orderRepository.GetPagedAsync(
            request.Page, request.PageSize,
            customerId: request.CustomerId,
            status: status,
            from: request.From,
            to: request.To,
            search: request.Search,
            email: request.Email,
            buyerName: null,
            ct: ct);

        // Cache registered customers to avoid N+1 (only for orders with a CustomerId)
        var customerIds = orders
            .Where(o => o.CustomerId.HasValue)
            .Select(o => o.CustomerId!.Value)
            .Distinct()
            .ToList();

        var customers = new Dictionary<Guid, string>();
        foreach (var cid in customerIds)
        {
            var c = await customerRepository.GetByIdAsync(cid, ct);
            if (c != null) customers[cid] = c.FullName;
        }

        var items = orders.Select(o => new OrderListItemDto(
            Id: o.Id,
            OrderNumber: o.OrderNumber,
            CustomerId: o.CustomerId,
            CustomerName: o.CustomerId.HasValue ? customers.GetValueOrDefault(o.CustomerId.Value) : null,
            BuyerName: o.BuyerName,
            BuyerEmail: o.BuyerEmail,
            Status: o.Status.ToString(),
            ItemCount: o.Items.Count,
            Total: o.Total.Amount,
            Currency: o.Total.Currency,
            PaidAt: o.PaidAt,
            CreatedAt: o.CreatedAt
        )).ToList();

        return PagedResult<OrderListItemDto>.Create(items, total, request.Page, request.PageSize);
    }
}
