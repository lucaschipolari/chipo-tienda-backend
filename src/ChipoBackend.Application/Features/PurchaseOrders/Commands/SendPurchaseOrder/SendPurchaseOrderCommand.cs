using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.PurchaseOrders.Commands.SendPurchaseOrder;

public record SendPurchaseOrderCommand(Guid Id) : IRequest;

public class SendPurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<SendPurchaseOrderCommand>
{
    public async Task Handle(SendPurchaseOrderCommand request, CancellationToken ct)
    {
        var order = await purchaseOrderRepository.GetWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException("Orden de compra", request.Id);

        order.Send();

        // Al enviar la orden, actualizar el costo de cada variante al costo de esta compra.
        // Así el "costo" del producto queda al día (para la ganancia real). No toca precio ni decants.
        var cache = new Dictionary<Guid, Product>();
        foreach (var item in order.Items)
        {
            if (item.UnitCost.Amount <= 0) continue;
            if (!cache.TryGetValue(item.ProductId, out var product))
            {
                var loaded = await productRepository.GetWithVariantsAsync(item.ProductId, ct);
                if (loaded is null) continue;
                product = loaded;
                cache[item.ProductId] = loaded;
            }
            if (product.IsDecant) continue;
            var variant = product.Variants.FirstOrDefault(v => v.Id == item.VariantId);
            variant?.UpdateCost(item.UnitCost);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
