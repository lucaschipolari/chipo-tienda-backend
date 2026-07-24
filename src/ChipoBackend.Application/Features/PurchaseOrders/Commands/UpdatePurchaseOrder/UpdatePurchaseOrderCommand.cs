using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.PurchaseOrders.DTOs;
using ChipoBackend.Domain.Entities.Purchasing;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.PurchaseOrders.Commands.UpdatePurchaseOrder;

/// <summary>Edita una orden de compra en estado Borrador (ítems + cabecera).</summary>
public record UpdatePurchaseOrderCommand(
    Guid Id,
    List<CreatePurchaseOrderItemRequest> Items,
    DateTime? ExpectedDeliveryDate = null,
    string Currency = "ARS",
    string? Notes = null
) : IRequest;

public class UpdatePurchaseOrderCommandValidator : AbstractValidator<UpdatePurchaseOrderCommand>
{
    public UpdatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
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

public class UpdatePurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdatePurchaseOrderCommand>
{
    public async Task Handle(UpdatePurchaseOrderCommand request, CancellationToken ct)
    {
        var order = await purchaseOrderRepository.GetWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException("Orden de compra", request.Id);

        if (order.Status != PurchaseOrderStatus.Draft)
            throw new ConflictException("Solo se pueden editar órdenes en estado Borrador.");

        order.UpdateHeader(request.ExpectedDeliveryDate, request.Notes, request.Currency);
        order.ClearItems();
        foreach (var item in request.Items)
            order.AddItem(item.ProductId, item.VariantId, item.Quantity, Money.Of(item.UnitCost, request.Currency));

        // Forzar estado Added de los ítems nuevos: tras Clear+Add, EF puede
        // detectarlos como Modified (UPDATE que afecta 0 filas). Igual que se hace
        // con StatusHistory en pedidos.
        foreach (var it in order.Items)
            unitOfWork.Add(it);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
