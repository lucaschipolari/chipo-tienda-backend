using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Products.Commands.DeleteProductVariant;

public record DeleteProductVariantCommand(Guid ProductId, Guid VariantId) : IRequest;

public class DeleteProductVariantCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteProductVariantCommand>
{
    public async Task Handle(DeleteProductVariantCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetWithVariantsAsync(request.ProductId, ct)
            ?? throw new NotFoundException($"Producto '{request.ProductId}' no encontrado.");

        product.RemoveVariant(request.VariantId);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
