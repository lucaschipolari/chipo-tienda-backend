using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Purchases.Commands.SendPurchaseOrder;

public record SendPurchaseOrderCommand(Guid Id) : IRequest;

public class SendPurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<SendPurchaseOrderCommand>
{
    public async Task Handle(SendPurchaseOrderCommand request, CancellationToken ct)
    {
        var order = await purchaseOrderRepository.GetWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException("Orden de compra", request.Id);

        order.Send();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
