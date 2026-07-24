using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.PurchaseOrders.Commands.DeletePurchaseOrder;

/// <summary>
/// Elimina físicamente una orden de compra (y sus ítems por cascada). Solo Admin.
/// Para limpiar pruebas o cargas erróneas. No revierte stock recibido.
/// </summary>
public record DeletePurchaseOrderCommand(Guid Id) : IRequest;

public class DeletePurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeletePurchaseOrderCommand>
{
    public async Task Handle(DeletePurchaseOrderCommand request, CancellationToken ct)
    {
        var order = await purchaseOrderRepository.GetWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException("Orden de compra", request.Id);

        purchaseOrderRepository.Remove(order);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
