using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Purchases.Commands.ApprovePurchaseOrder;

public record ApprovePurchaseOrderCommand(Guid Id) : IRequest;

public class ApprovePurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ApprovePurchaseOrderCommand>
{
    public async Task Handle(ApprovePurchaseOrderCommand request, CancellationToken ct)
    {
        var order = await purchaseOrderRepository.GetWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException("Orden de compra", request.Id);

        order.Approve();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
