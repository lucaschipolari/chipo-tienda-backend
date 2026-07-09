using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Inventory;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;


namespace ChipoBackend.Application.Features.Purchases.Commands.ReceivePurchaseOrder;

public record ReceivePurchaseOrderCommand(
    Guid Id,
    Dictionary<Guid, int> ItemReceipts
) : IRequest;

public class ReceivePurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IProductRepository productRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork
) : IRequestHandler<ReceivePurchaseOrderCommand>
{
    public async Task Handle(ReceivePurchaseOrderCommand request, CancellationToken ct)
    {
        await unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var order = await purchaseOrderRepository.GetWithItemsAsync(request.Id, ct)
                ?? throw new NotFoundException("Orden de compra", request.Id);

            // Snapshot quantities before domain method mutates them
            var itemsBefore = order.Items
                .Where(i => request.ItemReceipts.ContainsKey(i.Id))
                .ToDictionary(i => i.Id, i => (
                    item: i,
                    qtyReceivedBefore: i.QuantityReceived
                ));

            // Apply domain logic (validates status and quantities)
            order.ReceiveItems(request.ItemReceipts);

            // For each received item: increment stock and create movement
            foreach (var (itemId, qtyToReceive) in request.ItemReceipts)
            {
                if (!itemsBefore.TryGetValue(itemId, out var snapshot))
                    continue;

                var orderItem = snapshot.item;

                var product = await productRepository.GetWithVariantsAsync(orderItem.ProductId, ct)
                    ?? throw new NotFoundException("Producto", orderItem.ProductId);

                var variant = product.Variants.FirstOrDefault(v => v.Id == orderItem.VariantId)
                    ?? throw new NotFoundException($"Variante {orderItem.VariantId} no encontrada en el producto {orderItem.ProductId}.");

                var stockBefore = variant.StockQuantity;
                variant.IncrementStock(qtyToReceive);
                var stockAfter = variant.StockQuantity;

                var movement = StockMovement.Create(
                    productId: orderItem.ProductId,
                    variantId: orderItem.VariantId,
                    type: MovementType.PurchaseReceipt,
                    quantity: qtyToReceive,
                    stockBefore: stockBefore,
                    stockAfter: stockAfter,
                    referenceId: order.Id,
                    referenceType: "PurchaseOrder",
                    reason: $"Recepción de orden de compra {order.PurchaseNumber}",
                    createdByUserId: currentUserService.UserId
                );

                unitOfWork.Add(movement);
            }

            await unitOfWork.SaveChangesAsync(ct);
            await unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
