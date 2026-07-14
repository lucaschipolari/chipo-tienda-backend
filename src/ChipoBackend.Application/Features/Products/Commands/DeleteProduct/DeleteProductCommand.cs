using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest;

public class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetWithVariantsAsync(request.Id, ct)
            ?? throw new NotFoundException($"Producto '{request.Id}' no encontrado.");

        // Las variantes e imágenes se borran en cascada por su FK.
        productRepository.Remove(product);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
