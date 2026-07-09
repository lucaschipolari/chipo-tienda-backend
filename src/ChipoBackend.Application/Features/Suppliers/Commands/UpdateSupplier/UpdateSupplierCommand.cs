using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Suppliers.Commands.UpdateSupplier;

public record UpdateSupplierCommand(
    Guid Id,
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
) : IRequest;

public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
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

public class UpdateSupplierCommandHandler(
    ISupplierRepository supplierRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateSupplierCommand>
{
    public async Task Handle(UpdateSupplierCommand request, CancellationToken ct)
    {
        var supplier = await supplierRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Proveedor", request.Id);

        if (!string.IsNullOrWhiteSpace(request.TaxId) && request.TaxId != supplier.TaxId)
        {
            var existing = await supplierRepository.GetByTaxIdAsync(request.TaxId, ct);
            if (existing != null && existing.Id != request.Id)
                throw new ConflictException($"Ya existe un proveedor con el RUC/NIT '{request.TaxId}'.");
        }

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

        await unitOfWork.SaveChangesAsync(ct);
    }
}
