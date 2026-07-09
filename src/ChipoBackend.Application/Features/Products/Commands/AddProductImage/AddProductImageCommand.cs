using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Products.Commands.AddProductImage;

public record AddProductImageCommand(
    Guid ProductId,
    string Url,
    string? AltText = null
) : IRequest<Guid>;

public class AddProductImageCommandValidator : AbstractValidator<AddProductImageCommand>
{
    public AddProductImageCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Url).NotEmpty().MaximumLength(1000);
    }
}

public class AddProductImageCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<AddProductImageCommand, Guid>
{
    public async Task<Guid> Handle(AddProductImageCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetWithVariantsAsync(request.ProductId, ct)
            ?? throw new NotFoundException($"Producto '{request.ProductId}' no encontrado.");

        // La nueva imagen va al final del orden actual.
        var nextOrder = product.Images.Count == 0 ? 0 : product.Images.Max(i => i.DisplayOrder) + 1;
        product.AddImage(ImageUrlHelper.Normalize(request.Url), request.AltText, nextOrder);

        await unitOfWork.SaveChangesAsync(ct);

        // Id de la imagen recién agregada (la de mayor DisplayOrder).
        return product.Images.OrderByDescending(i => i.DisplayOrder).First().Id;
    }
}
