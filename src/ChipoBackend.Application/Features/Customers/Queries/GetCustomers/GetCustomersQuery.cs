using ChipoBackend.Application.Common.Models;
using ChipoBackend.Application.Features.Customers.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Customers.Queries.GetCustomers;

public record GetCustomersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool? IsActive = null
) : IRequest<PagedResult<CustomerListItemDto>>;

public class GetCustomersQueryHandler(
    ICustomerRepository customerRepository,
    IOrderRepository orderRepository,
    ISaleRepository saleRepository
) : IRequestHandler<GetCustomersQuery, PagedResult<CustomerListItemDto>>
{
    public async Task<PagedResult<CustomerListItemDto>> Handle(GetCustomersQuery request, CancellationToken ct)
    {
        var (customers, total) = await customerRepository.GetPagedAsync(
            request.Page, request.PageSize, request.Search, request.IsActive, ct);

        var items = new List<CustomerListItemDto>();
        foreach (var c in customers)
        {
            var (allOrders, _) = await orderRepository.GetPagedAsync(1, 1000, c.Id, ct: ct);
            var orders = allOrders.Where(o => o.Status != Domain.Entities.Orders.OrderStatus.Cancelled).ToList();
            var orderCount = orders.Count;
            var (sales, saleCount) = await saleRepository.GetPagedAsync(1, 1000, customerId: c.Id, ct: ct);
            var totalSpent = orders.Sum(o => o.Total.Amount) + sales.Sum(s => s.Total.Amount);
            var lastOrderAt = orders.Select(o => (DateTime?)o.CreatedAt)
                .Concat(sales.Select(s => (DateTime?)s.CreatedAt))
                .Max();

            items.Add(new CustomerListItemDto(
                Id: c.Id,
                FullName: c.FullName,
                Email: c.Email,
                PhoneNumber: c.PhoneNumber,
                DocumentNumber: c.DocumentNumber,
                DocumentType: c.DocumentType.ToString(),
                City: c.City,
                CustomerType: c.CustomerType.ToString(),
                IsActive: c.IsActive,
                TotalOrders: orderCount + saleCount,
                TotalSpent: totalSpent,
                Currency: "ARS",
                LastOrderAt: lastOrderAt,
                CreatedAt: c.CreatedAt
            ));
        }

        return PagedResult<CustomerListItemDto>.Create(items, total, request.Page, request.PageSize);
    }
}
