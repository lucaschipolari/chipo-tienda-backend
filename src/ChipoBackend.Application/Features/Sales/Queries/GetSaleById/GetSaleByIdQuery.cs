using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Sales.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Sales.Queries.GetSaleById;

public record GetSaleByIdQuery(Guid Id) : IRequest<SaleDto>;

public class GetSaleByIdQueryHandler(
    ISaleRepository saleRepository,
    ICustomerRepository customerRepository
) : IRequestHandler<GetSaleByIdQuery, SaleDto>
{
    public async Task<SaleDto> Handle(GetSaleByIdQuery request, CancellationToken ct)
    {
        var sale = await saleRepository.GetWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException($"Venta '{request.Id}' no encontrada.");

        string? customerName = null;
        if (sale.CustomerId.HasValue)
        {
            var customer = await customerRepository.GetByIdAsync(sale.CustomerId.Value, ct);
            customerName = customer?.FullName;
        }

        return new SaleDto(
            Id: sale.Id,
            SaleNumber: sale.SaleNumber,
            CustomerId: sale.CustomerId,
            CustomerName: customerName ?? sale.CustomerName,
            SoldByUserId: sale.SoldByUserId,
            SoldByUserName: null, // Se puede enriquecer con IUserRepository si se necesita
            Channel: sale.Channel.ToString(),
            Subtotal: sale.Subtotal.Amount,
            DiscountAmount: sale.DiscountAmount.Amount,
            Total: sale.Total.Amount,
            TotalCost: sale.TotalCost.Amount,
            Profit: sale.Profit.Amount,
            Currency: sale.Total.Currency,
            PaymentMethod: sale.PaymentMethod,
            Notes: sale.Notes,
            Items: sale.Items.Select(i => new SaleItemDto(
                Id: i.Id,
                ProductId: i.ProductId,
                VariantId: i.VariantId,
                ProductName: i.ProductName,
                Sku: i.Sku,
                Quantity: i.Quantity,
                UnitPrice: i.UnitPrice.Amount,
                Discount: i.Discount.Amount,
                Total: i.Total.Amount,
                UnitCost: i.UnitCost.Amount,
                TotalCost: i.TotalCost.Amount,
                Profit: i.Profit.Amount,
                Currency: i.UnitPrice.Currency
            )).ToList(),
            CreatedAt: sale.CreatedAt
        );
    }
}
