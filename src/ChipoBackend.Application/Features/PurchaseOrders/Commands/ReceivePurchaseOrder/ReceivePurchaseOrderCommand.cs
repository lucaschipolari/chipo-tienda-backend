using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Inventory;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.PurchaseOrders.Commands.ReceivePurchaseOrder;

public record ReceivePurchaseOrderCommand(
    Guid Id,
    Dictionary<Guid, int> ItemReceipts
) : IRequest;

public class ReceivePurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IProductRepository productRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork
) : IRequestHandler<ReceivePurchaseOrderCommand>
{
    public async Task Handle(ReceivePurchaseOrderCommand request, CancellationToken ct)
    {
        var order = await purchaseOrderRepository.GetWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException("Orden de compra", request.Id);

        order.ReceiveItems(request.ItemReceipts);

        // Incrementar stock por cada ítem recibido y registrar el movimiento
        foreach (var (itemId, quantityReceived) in request.ItemReceipts)
        {
            if (quantityReceived <= 0) continue;

            var item = order.Items.First(i => i.Id == itemId);
            var product = await productRepository.GetWithVariantsAsync(item.ProductId, ct);
            var variant = product?.Variants.FirstOrDefault(v => v.Id == item.VariantId)
                ?? throw new NotFoundException($"Variante '{item.VariantId}' no encontrada.");

            var stockBefore = variant.StockQuantity;
            variant.IncrementStock(quantityReceived);

            var movement = StockMovement.Create(
                item.ProductId, item.VariantId, MovementType.PurchaseReceipt,
                quantityReceived, stockBefore, variant.StockQuantity,
                referenceId: order.Id,
                referenceType: "PurchaseOrder",
                reason: $"Recepción {order.PurchaseNumber}",
                createdByUserId: currentUser.UserId);
            unitOfWork.Add(movement);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
