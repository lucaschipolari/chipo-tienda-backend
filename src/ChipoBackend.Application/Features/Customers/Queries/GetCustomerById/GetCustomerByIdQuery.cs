using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Customers.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto>;

public class GetCustomerByIdQueryHandler(
    ICustomerRepository customerRepository,
    IOrderRepository orderRepository,
    ISaleRepository saleRepository
) : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken ct)
    {
        var customer = await customerRepository.GetWithAddressesAsync(request.Id, ct)
            ?? throw new NotFoundException($"Cliente '{request.Id}' no encontrado.");

        var (allOrders, _) = await orderRepository.GetPagedAsync(1, 1000, request.Id, ct: ct);
        var orders = allOrders.Where(o => o.Status != Domain.Entities.Orders.OrderStatus.Cancelled).ToList();
        var orderCount = orders.Count;
        var (sales, saleCount) = await saleRepository.GetPagedAsync(1, 1000, customerId: request.Id, ct: ct);
        var totalSpent = orders.Sum(o => o.Total.Amount) + sales.Sum(s => s.Total.Amount);
        var lastOrderAt = orders.Select(o => (DateTime?)o.CreatedAt)
            .Concat(sales.Select(s => (DateTime?)s.CreatedAt))
            .Max();

        return new CustomerDto(
            Id: customer.Id,
            UserId: customer.UserId,
            FirstName: customer.FirstName,
            LastName: customer.LastName,
            FullName: customer.FullName,
            Email: customer.Email,
            PhoneNumber: customer.PhoneNumber,
            DocumentNumber: customer.DocumentNumber,
            DocumentType: customer.DocumentType.ToString(),
            Street: customer.Street,
            City: customer.City,
            Province: customer.Province,
            PostalCode: customer.PostalCode,
            CustomerType: customer.CustomerType.ToString(),
            IsActive: customer.IsActive,
            Notes: customer.Notes,
            Addresses: customer.Addresses.Select(a => new CustomerAddressDto(
                Id: a.Id,
                Label: a.Label,
                Street: a.Address.Street,
                City: a.Address.City,
                State: a.Address.State,
                PostalCode: a.Address.PostalCode,
                Country: a.Address.Country,
                IsDefault: a.IsDefault
            )).ToList(),
            TotalOrders: orderCount + saleCount,
            TotalSpent: totalSpent,
            Currency: "ARS",
            LastOrderAt: lastOrderAt,
            CreatedAt: customer.CreatedAt,
            UpdatedAt: customer.UpdatedAt
        );
    }
}
