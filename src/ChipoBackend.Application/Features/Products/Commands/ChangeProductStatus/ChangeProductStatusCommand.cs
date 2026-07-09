using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;
using ValidationException = FluentValidation.ValidationException;

namespace ChipoBackend.Application.Features.Products.Commands.ChangeProductStatus;

public record ChangeProductStatusCommand(Guid Id, string Status) : IRequest;

public class ChangeProductStatusCommandValidator : AbstractValidator<ChangeProductStatusCommand>
{
    private static readonly string[] ValidStatuses = ["Draft", "Published", "Discontinued"];

    public ChangeProductStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status)
            .Must(s => ValidStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Estado inválido. Valores permitidos: {string.Join(", ", ValidStatuses)}.");
    }
}

public class ChangeProductStatusCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ChangeProductStatusCommand>
{
    public async Task Handle(ChangeProductStatusCommand request, CancellationToken ct)
    {
        var product = await productRepository.GetWithVariantsAsync(request.Id, ct)
            ?? throw new NotFoundException($"Producto '{request.Id}' no encontrado.");

        switch (request.Status.ToLowerInvariant())
        {
            case "published":    product.Publish(); break;
            case "discontinued": product.Discontinue(); break;
            case "draft":        product.SetAsDraft(); break;
            default: throw new ConflictException($"Estado desconocido: {request.Status}");
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
