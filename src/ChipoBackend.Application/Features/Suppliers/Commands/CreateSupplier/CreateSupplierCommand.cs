using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Entities.Purchasing;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Suppliers.Commands.CreateSupplier;

public record CreateSupplierCommand(
    string CompanyName,
    string? TradeName,
    string? TaxId,
    string? Email,
    string? Phone,
    string? Website,
    string? City,
    string? Province,
    string? Country,
    string? PaymentTerms,
    string? Notes
) : IRequest<Guid>;

public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().MaximumLength(200)
            .WithMessage("El nombre de la empresa es requerido (máx 200 caracteres).");

        RuleFor(x => x.TaxId)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.TaxId))
            .WithMessage("El RUC/NIT no puede superar 20 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("El email no tiene formato válido.");
    }
}

public class CreateSupplierCommandHandler(
    ISupplierRepository supplierRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateSupplierCommand, Guid>
{
    public async Task<Guid> Handle(CreateSupplierCommand request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.TaxId))
        {
            var existing = await supplierRepository.GetByTaxIdAsync(request.TaxId, ct);
            if (existing != null)
                throw new ConflictException($"Ya existe un proveedor con el RUC/NIT '{request.TaxId}'.");
        }

        var supplier = Supplier.Create(
            request.CompanyName,
            contactName: null,
            request.Email,
            request.Phone,
            request.TradeName,
            request.Website,
            request.City,
            request.Province,
            request.Country
        );

        supplier.Update(
            request.CompanyName,
            contactName: null,
            request.Email,
            request.Phone,
            request.TaxId,
            address: null,
            request.PaymentTerms,
            request.Notes,
            request.TradeName,
            request.Website,
            request.City,
            request.Province,
            request.Country
        );

        unitOfWork.Add(supplier);
        await unitOfWork.SaveChangesAsync(ct);
        return supplier.Id;
    }
}
