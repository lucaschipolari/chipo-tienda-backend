using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Products.Commands.ConfigureDecant;

/// <summary>Configura un producto como decant (por ml): frasco fuente, stock en ml y aviso de reposición.</summary>
public record ConfigureDecantCommand(
    Guid ProductId,
    decimal? BottleCost,
    int? BottleMl,
    int StockMl,
    int ReorderMl
) : IRequest;

public class ConfigureDecantCommandValidator : AbstractValidator<ConfigureDecantCommand>
{
    public ConfigureDecantCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.StockMl).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReorderMl).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BottleMl).GreaterThan(0).When(x => x.BottleMl.HasValue);
        RuleFor(x => x.BottleCost).GreaterThanOrEqualTo(0).When(x => x.BottleCost.HasValue);
    }
}

public class ConfigureDecantCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ConfigureDecantCommand>
{
    public async Task Handle(ConfigureDecantCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetWithVariantsAsync(request.ProductId, ct)
            ?? throw new NotFoundException($"Producto '{request.ProductId}' no encontrado.");

        product.ConfigureDecant(request.BottleCost, request.BottleMl, request.ReorderMl);
        product.SetStockMl(request.StockMl);

        await unitOfWork.SaveChangesAsync(ct);
    }
}
