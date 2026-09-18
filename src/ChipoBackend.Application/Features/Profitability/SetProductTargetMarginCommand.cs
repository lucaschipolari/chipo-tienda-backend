using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Profitability;

/// <summary>Define (o limpia con null) el margen objetivo específico de un producto.</summary>
public record SetProductTargetMarginCommand(Guid ProductId, decimal? TargetMarginPct) : IRequest;

public class SetProductTargetMarginCommandValidator : AbstractValidator<SetProductTargetMarginCommand>
{
    public SetProductTargetMarginCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.TargetMarginPct)
            .InclusiveBetween(0m, 99.99m)
            .When(x => x.TargetMarginPct.HasValue)
            .WithMessage("El margen objetivo debe estar entre 0 y 99,99%.");
    }
}

public class SetProductTargetMarginCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<SetProductTargetMarginCommand>
{
    public async Task Handle(SetProductTargetMarginCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Producto", request.ProductId);

        product.SetTargetMargin(request.TargetMarginPct);
        productRepository.Update(product);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
