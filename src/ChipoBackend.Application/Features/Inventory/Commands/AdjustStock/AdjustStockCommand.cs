using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Inventory;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Inventory.Commands.AdjustStock;

public record AdjustStockCommand(
    Guid ProductId,
    Guid VariantId,
    int NewQuantity,
    string Reason
) : IRequest;

public class AdjustStockCommandValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.NewQuantity).GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser negativa.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500).WithMessage("El motivo es requerido.");
    }
}

public class AdjustStockCommandHandler(
    IProductRepository productRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork
) : IRequestHandler<AdjustStockCommand>
{
    public async Task Handle(AdjustStockCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetWithVariantsAsync(request.ProductId, ct)
            ?? throw new NotFoundException($"Producto '{request.ProductId}' no encontrado.");

        var variant = product.Variants.FirstOrDefault(v => v.Id == request.VariantId)
            ?? throw new NotFoundException($"Variante '{request.VariantId}' no encontrada en el producto.");

        var stockBefore = variant.StockQuantity;
        var delta = request.NewQuantity - stockBefore;

        variant.UpdateStock(request.NewQuantity);

        var movementType = delta >= 0 ? MovementType.ManualAdjustment : MovementType.ManualAdjustment;
        var movement = StockMovement.Create(
            product.Id, variant.Id, movementType,
            Math.Abs(delta), stockBefore, request.NewQuantity,
            reason: request.Reason,
            createdByUserId: currentUser.UserId);

        unitOfWork.Add(movement);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
