using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Purchasing;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Purchases.Commands.CreatePurchaseOrder;

public record CreatePurchaseOrderItemCommand(
    Guid ProductId,
    Guid VariantId,
    int Quantity,
    decimal UnitCost
);

public record CreatePurchaseOrderCommand(
    Guid SupplierId,
    DateTime? ExpectedDeliveryDate,
    string Currency,
    string? Notes,
    List<CreatePurchaseOrderItemCommand> Items
) : IRequest<Guid>;

public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("El proveedor es requerido.");

        RuleFor(x => x.Currency)
            .NotEmpty().MaximumLength(3)
            .WithMessage("La moneda es requerida (máx 3 caracteres).");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La orden debe contener al menos un ítem.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");
            item.RuleFor(i => i.UnitCost)
                .GreaterThanOrEqualTo(0).WithMessage("El costo unitario no puede ser negativo.");
        });
    }
}

public class CreatePurchaseOrderCommandHandler(
    ISupplierRepository supplierRepository,
    IProductRepository productRepository,
    IPurchaseOrderRepository purchaseOrderRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreatePurchaseOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreatePurchaseOrderCommand request, CancellationToken ct)
    {
        var supplier = await supplierRepository.GetByIdAsync(request.SupplierId, ct)
            ?? throw new NotFoundException("Proveedor", request.SupplierId);

        if (!supplier.IsActive)
            throw new ConflictException("El proveedor no está activo.");

        // Validate products/variants exist
        foreach (var item in request.Items)
        {
            var product = await productRepository.GetWithVariantsAsync(item.ProductId, ct)
                ?? throw new NotFoundException("Producto", item.ProductId);

            var variant = product.Variants.FirstOrDefault(v => v.Id == item.VariantId)
                ?? throw new NotFoundException($"Variante {item.VariantId} no encontrada en el producto {item.ProductId}.");
        }

        var purchaseNumber = await purchaseOrderRepository.GeneratePurchaseNumberAsync(ct);

        var order = PurchaseOrder.Create(
            purchaseNumber,
            request.SupplierId,
            currentUserService.UserId,
            request.ExpectedDeliveryDate
        );

        foreach (var item in request.Items)
        {
            var unitCost = Money.Of(item.UnitCost, request.Currency);
            order.AddItem(item.ProductId, item.VariantId, item.Quantity, unitCost);
        }

        unitOfWork.Add(order);
        await unitOfWork.SaveChangesAsync(ct);
        return order.Id;
    }
}
