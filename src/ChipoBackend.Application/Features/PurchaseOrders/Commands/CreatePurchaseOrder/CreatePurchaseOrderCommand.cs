using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Application.Features.PurchaseOrders.DTOs;
using ChipoBackend.Domain.Entities.Purchasing;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;


namespace ChipoBackend.Application.Features.PurchaseOrders.Commands.CreatePurchaseOrder;

public record CreatePurchaseOrderCommand(
    Guid SupplierId,
    List<CreatePurchaseOrderItemRequest> Items,
    DateTime? ExpectedDeliveryDate = null,
    string Currency = "ARS",
    string? Notes = null
) : IRequest<Guid>;

public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty().WithMessage("El proveedor es requerido.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("La orden debe tener al menos un ítem.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.VariantId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");
            item.RuleFor(i => i.UnitCost).GreaterThanOrEqualTo(0).WithMessage("El costo unitario no puede ser negativo.");
        });
    }
}

public class CreatePurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    ISupplierRepository supplierRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreatePurchaseOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreatePurchaseOrderCommand request, CancellationToken ct)
    {
        var supplier = await supplierRepository.GetByIdAsync(request.SupplierId, ct)
            ?? throw new NotFoundException("Proveedor", request.SupplierId);

        if (!supplier.IsActive)
            throw new ConflictException("El proveedor está inactivo.");

        var purchaseNumber = await purchaseOrderRepository.GeneratePurchaseNumberAsync(ct);
        var userId = currentUserService.UserId;

        var order = PurchaseOrder.Create(purchaseNumber, request.SupplierId, userId, request.ExpectedDeliveryDate, request.Currency);

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
