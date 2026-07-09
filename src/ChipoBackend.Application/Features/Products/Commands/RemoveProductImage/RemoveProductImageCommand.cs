using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Products.Commands.RemoveProductImage;

public record RemoveProductImageCommand(
    Guid ProductId,
    Guid ImageId
) : IRequest;

public class RemoveProductImageCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<RemoveProductImageCommand>
{
    public async Task Handle(RemoveProductImageCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetWithVariantsAsync(request.ProductId, ct)
            ?? throw new NotFoundException($"Producto '{request.ProductId}' no encontrado.");

        product.RemoveImage(request.ImageId);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
