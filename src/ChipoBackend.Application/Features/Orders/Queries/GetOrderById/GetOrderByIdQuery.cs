using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Orders.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDto>;

public class GetOrderByIdQueryHandler(
    IOrderRepository orderRepository,
    ICustomerRepository customerRepository
) : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var order = await orderRepository.GetWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException($"Pedido '{request.Id}' no encontrado.");

        // Only look up the customer record if this is a registered-customer order
        string? customerName = null;
        string? customerEmail = null;
        if (order.CustomerId.HasValue)
        {
            var customer = await customerRepository.GetByIdAsync(order.CustomerId.Value, ct);
            customerName = customer?.FullName;
            customerEmail = customer?.Email;
        }

        return new OrderDto(
            Id: order.Id,
            OrderNumber: order.OrderNumber,
            CustomerId: order.CustomerId,
            CustomerName: customerName,
            CustomerEmail: customerEmail,
            BuyerName: order.BuyerName,
            BuyerEmail: order.BuyerEmail,
            BuyerPhone: order.BuyerPhone,
            PaymentMethod: order.PaymentMethod,
            DeliveryMethod: order.DeliveryMethod,
            Status: order.Status.ToString(),
            ShippingAddress: new OrderAddressDto(
                order.ShippingAddress.Street, order.ShippingAddress.City,
                order.ShippingAddress.State, order.ShippingAddress.PostalCode,
                order.ShippingAddress.Country),
            BillingAddress: order.BillingAddress == null ? null : new OrderAddressDto(
                order.BillingAddress.Street, order.BillingAddress.City,
                order.BillingAddress.State, order.BillingAddress.PostalCode,
                order.BillingAddress.Country),
            Subtotal: order.Subtotal.Amount,
            DiscountAmount: order.DiscountAmount.Amount,
            ShippingCost: order.ShippingCost.Amount,
            TaxAmount: order.TaxAmount.Amount,
            Total: order.Total.Amount,
            Currency: order.Total.Currency,
            CouponCode: order.CouponCode,
            Notes: order.Notes,
            CancelReason: order.CancelReason,
            PaidAt: order.PaidAt,
            ShippedAt: order.ShippedAt,
            DeliveredAt: order.DeliveredAt,
            CancelledAt: order.CancelledAt,
            Items: order.Items.Select(i => new OrderItemDto(
                Id: i.Id,
                ProductId: i.ProductId,
                VariantId: i.VariantId,
                ProductName: i.ProductName,
                VariantDescription: i.VariantDescription,
                Sku: i.Sku,
                Quantity: i.Quantity,
                UnitPrice: i.UnitPrice.Amount,
                Discount: i.Discount.Amount,
                Total: i.Total.Amount,
                Currency: i.UnitPrice.Currency
            )).ToList(),
            StatusHistory: order.StatusHistory.OrderBy(h => h.ChangedAt).Select(h => new OrderStatusHistoryDto(
                FromStatus: h.FromStatus?.ToString(),
                ToStatus: h.ToStatus.ToString(),
                Note: h.Note,
                ChangedByUserId: h.ChangedByUserId,
                ChangedAt: h.ChangedAt
            )).ToList(),
            Payments: order.Payments.Select(p => new PaymentDto(
                Id: p.Id,
                Method: p.Method,
                Amount: p.Amount.Amount,
                Currency: p.Amount.Currency,
                Status: p.Status.ToString(),
                GatewayRef: p.GatewayRef,
                ProcessedAt: p.ProcessedAt,
                CreatedAt: p.CreatedAt
            )).ToList(),
            CreatedAt: order.CreatedAt,
            UpdatedAt: order.UpdatedAt
        );
    }
}
