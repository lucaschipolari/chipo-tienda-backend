using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Inventory;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using ChipoBackend.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Products.Commands.AddProductVariant;

public record AddProductVariantCommand(
    Guid ProductId,
    string Sku,
    Dictionary<string, string> Attributes,
    int InitialStock,
    decimal? Price,
    string Currency,
    int MinStockThreshold,
    decimal? CompareAtPrice = null,
    decimal? Cost = null
) : IRequest<Guid>;

public class AddProductVariantCommandValidator : AbstractValidator<AddProductVariantCommand>
{
    public AddProductVariantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(100);
        RuleFor(x => x.InitialStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinStockThreshold).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
    }
}

public class AddProductVariantCommandHandler(
    IProductRepository productRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork
) : IRequestHandler<AddProductVariantCommand, Guid>
{
    public async Task<Guid> Handle(AddProductVariantCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetWithVariantsAsync(request.ProductId, ct)
            ?? throw new NotFoundException($"Producto '{request.ProductId}' no encontrado.");

        var price = request.Price.HasValue ? Money.Of(request.Price.Value, request.Currency) : null;
        var compareAt = request.CompareAtPrice.HasValue ? Money.Of(request.CompareAtPrice.Value, request.Currency) : null;
        var cost = request.Cost.HasValue ? Money.Of(request.Cost.Value, request.Currency) : null;
        var variant = product.AddVariant(request.Sku, request.Attributes ?? [], request.InitialStock, price, request.MinStockThreshold, compareAt, cost);
        unitOfWork.Add(variant);

        if (request.InitialStock > 0)
        {
            var movement = StockMovement.Create(
                product.Id, variant.Id, MovementType.Initial,
                request.InitialStock, 0, request.InitialStock,
                reason: "Stock inicial al agregar variante",
                createdByUserId: currentUser.UserId);
            unitOfWork.Add(movement);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return variant.Id;
    }
}
