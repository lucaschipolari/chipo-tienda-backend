using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Products.Commands.UpdateProductVariant;

public record UpdateProductVariantCommand(
    Guid ProductId,
    Guid VariantId,
    decimal? Price,
    string Currency,
    int MinStockThreshold,
    bool IsActive,
    decimal? CompareAtPrice = null,
    decimal? Cost = null
) : IRequest;

public class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
{
    public UpdateProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.MinStockThreshold).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
    }
}

public class UpdateProductVariantCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateProductVariantCommand>
{
    public async Task Handle(UpdateProductVariantCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetWithVariantsAsync(request.ProductId, ct)
            ?? throw new NotFoundException($"Producto '{request.ProductId}' no encontrado.");

        var variant = product.Variants.FirstOrDefault(v => v.Id == request.VariantId)
            ?? throw new NotFoundException($"Variante '{request.VariantId}' no encontrada en el producto.");

        var price = request.Price.HasValue ? Money.Of(request.Price.Value, request.Currency) : null;
        variant.UpdatePrice(price);

        var compareAt = request.CompareAtPrice.HasValue ? Money.Of(request.CompareAtPrice.Value, request.Currency) : null;
        variant.UpdateCompareAtPrice(compareAt);

        var cost = request.Cost.HasValue ? Money.Of(request.Cost.Value, request.Currency) : null;
        variant.UpdateCost(cost);

        if (request.IsActive && !variant.IsActive) variant.Activate();
        else if (!request.IsActive && variant.IsActive) variant.Deactivate();

        await unitOfWork.SaveChangesAsync(ct);
    }
}
