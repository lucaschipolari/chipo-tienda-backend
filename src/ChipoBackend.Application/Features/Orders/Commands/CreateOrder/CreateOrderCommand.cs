using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Orders.DTOs;
using ChipoBackend.Domain.Entities.Orders;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    Guid? CustomerId,
    string BuyerName,
    string BuyerEmail,
    string? BuyerPhone,
    List<CreateOrderItemRequest> Items,
    CreateOrderAddressRequest ShippingAddress,
    CreateOrderAddressRequest? BillingAddress,
    decimal ShippingCost,
    string Currency,
    string? PaymentMethod,
    string? DeliveryMethod,
    string? CouponCode,
    string? Notes
) : IRequest<Guid>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.BuyerName).NotEmpty().WithMessage("El nombre del comprador es requerido.");
        RuleFor(x => x.BuyerEmail).NotEmpty().EmailAddress().WithMessage("El email del comprador es requerido y debe ser válido.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("El pedido debe tener al menos un ítem.");
        RuleFor(x => x.Currency).NotEmpty()
            .Must(c => c == "ARS" || c == "USD")
            .WithMessage("La moneda debe ser ARS (peso argentino) o USD (dólar).");
        RuleFor(x => x.ShippingCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ShippingAddress).NotNull().WithMessage("La dirección de envío es requerida.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.VariantId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");
        });
    }
}

public class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    ICustomerRepository customerRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        string buyerName = request.BuyerName;
        string buyerEmail = request.BuyerEmail;

        // If a registered customer is provided, validate and use their data
        if (request.CustomerId.HasValue)
        {
            var customer = await customerRepository.GetByIdAsync(request.CustomerId.Value, ct)
                ?? throw new NotFoundException($"Cliente '{request.CustomerId.Value}' no encontrado.");

            if (!customer.IsActive)
                throw new ConflictException("No se puede crear un pedido para un cliente inactivo.");

            // Prefer customer record data over supplied buyer info
            buyerName = customer.FullName ?? buyerName;
            buyerEmail = customer.Email ?? buyerEmail;
        }

        // Validate stock and resolve item details
        var resolvedItems = new List<(CreateOrderItemRequest Req, string ProductName, string VariantDesc, string Sku, Money Price)>();
        foreach (var itemReq in request.Items)
        {
            var product = await productRepository.GetWithVariantsAsync(itemReq.ProductId, ct)
                ?? throw new NotFoundException($"Producto '{itemReq.ProductId}' no encontrado.");

            var variant = product.Variants.FirstOrDefault(v => v.Id == itemReq.VariantId)
                ?? throw new NotFoundException($"Variante '{itemReq.VariantId}' no encontrada.");

            if (!variant.IsActive)
                throw new ConflictException($"La variante '{variant.Sku}' no está activa.");

            if (variant.StockQuantity < itemReq.Quantity)
                throw new ConflictException($"Stock insuficiente para '{variant.Sku}'. Disponible: {variant.StockQuantity}, solicitado: {itemReq.Quantity}.");

            // Always express the price in the order's requested currency.
            // Treat variant.Price = 0 as "not set" and fall back to product.BasePrice.
            var variantAmount = variant.Price?.Amount;
            var rawPrice = itemReq.UnitPriceOverride
                ?? (variantAmount > 0 ? variantAmount : null)
                ?? product.BasePrice.Amount;
            var price = Money.Of(rawPrice, request.Currency);

            var variantDesc = string.Join(", ", variant.Attributes.Select(kv => $"{kv.Key}: {kv.Value}"));
            resolvedItems.Add((itemReq, product.Name, variantDesc, variant.Sku, price));
        }

        // Generate order number
        var orderNumber = await orderRepository.GenerateOrderNumberAsync(ct);

        // Build shipping address
        var shippingAddr = new Address(
            request.ShippingAddress.Street, request.ShippingAddress.City,
            request.ShippingAddress.State, request.ShippingAddress.PostalCode,
            request.ShippingAddress.Country);

        var order = Order.Create(
            orderNumber,
            buyerName,
            buyerEmail,
            shippingAddr,
            customerId: request.CustomerId,
            buyerPhone: request.BuyerPhone,
            paymentMethod: request.PaymentMethod,
            deliveryMethod: request.DeliveryMethod,
            notes: request.Notes,
            currency: request.Currency);

        unitOfWork.Add(order);

        // Add items
        foreach (var (req, productName, variantDesc, sku, price) in resolvedItems)
        {
            var discount = Money.Of(req.Discount, request.Currency);
            order.AddItem(req.ProductId, req.VariantId, productName, variantDesc, sku, req.Quantity, price, discount);
        }

        // Apply shipping cost
        if (request.ShippingCost > 0)
            order.SetShippingCost(Money.Of(request.ShippingCost, request.Currency));

        // Apply coupon if provided (placeholder — coupon logic can be extended)
        // if (!string.IsNullOrWhiteSpace(request.CouponCode)) { ... }

        await unitOfWork.SaveChangesAsync(ct);
        return order.Id;
    }
}
