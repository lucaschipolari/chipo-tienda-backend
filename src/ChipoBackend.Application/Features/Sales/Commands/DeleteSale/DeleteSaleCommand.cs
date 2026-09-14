using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Entities.Inventory;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Sales.Commands.DeleteSale;

public record DeleteSaleCommand(Guid Id) : IRequest;

public class DeleteSaleCommandHandler(
    ISaleRepository saleRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteSaleCommand>
{
    public async Task Handle(DeleteSaleCommand request, CancellationToken ct)
    {
        var sale = await saleRepository.GetWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException($"Venta '{request.Id}' no encontrada.");

        // Las ventas generadas desde un pedido NO descontaron stock ellas mismas
        // (lo hizo el pedido al confirmarse), así que no se restaura en ese caso.
        // Devolver el stock de cada ítem (la venta lo había descontado al crearse).
        var items = sale.OrderId.HasValue ? Enumerable.Empty<Domain.Entities.Sales.SaleItem>() : sale.Items;
        foreach (var item in items)
        {
            var product = await productRepository.GetWithVariantsAsync(item.ProductId, ct);
            if (product == null) continue;
            var variant = product.Variants.FirstOrDefault(v => v.Id == item.VariantId);
            if (variant == null) continue;

            if (product.IsDecant)
            {
                // Decant: devuelve los ml al pool del frasco.
                var mlBack = ParseMl(variant.Attributes) * item.Quantity;
                if (mlBack <= 0) continue;
                var before = product.StockMl;
                product.SetStockMl(product.StockMl + mlBack);
                unitOfWork.Add(StockMovement.Create(
                    product.Id, variant.Id, MovementType.Return,
                    mlBack, before, product.StockMl,
                    referenceId: sale.Id, referenceType: "Sale",
                    reason: $"Venta {sale.SaleNumber} eliminada — {mlBack} ml restaurados",
                    createdByUserId: null));
            }
            else
            {
                var before = variant.StockQuantity;
                variant.IncrementStock(item.Quantity);
                unitOfWork.Add(StockMovement.Create(
                    product.Id, variant.Id, MovementType.Return,
                    item.Quantity, before, variant.StockQuantity,
                    referenceId: sale.Id, referenceType: "Sale",
                    reason: $"Venta {sale.SaleNumber} eliminada — stock restaurado",
                    createdByUserId: null));
            }
        }

        saleRepository.Remove(sale);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static int ParseMl(Dictionary<string, string> attributes)
    {
        if (attributes == null) return 0;
        foreach (var v in attributes.Values)
        {
            var m = System.Text.RegularExpressions.Regex.Match(v ?? "", @"(\d+)\s*ml",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (m.Success) return int.Parse(m.Groups[1].Value);
        }
        return 0;
    }
}
