using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : IRequest;

public class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Categoría '{request.Id}' no encontrada.");

        // Verificar que no tiene productos
        var (products, count) = await productRepository.GetPagedAsync(1, 1, request.Id, ct: ct);
        if (count > 0)
            throw new ConflictException("No se puede eliminar una categoría que contiene productos. Reasigna o elimina los productos primero.");

        // Desactivar en lugar de eliminar (soft delete)
        category.Deactivate();
        await unitOfWork.SaveChangesAsync(ct);
    }
}
