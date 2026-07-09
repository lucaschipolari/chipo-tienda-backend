using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Entities.Purchasing;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.PurchaseOrders.Commands.CancelPurchaseOrder;

public record CancelPurchaseOrderCommand(Guid Id) : IRequest;

public class CancelPurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CancelPurchaseOrderCommand>
{
    public async Task Handle(CancelPurchaseOrderCommand request, CancellationToken ct)
    {
        var order = await purchaseOrderRepository.GetWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException("Orden de compra", request.Id);

        if (order.Status != PurchaseOrderStatus.Draft)
            throw new ConflictException("Solo se pueden cancelar órdenes en estado Borrador.");

        order.Cancel();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
