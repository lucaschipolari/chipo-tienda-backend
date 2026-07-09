using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Entities.Customers;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string DocumentNumber,
    string DocumentType,        // "DNI" | "RUC" | "CE" | "Pasaporte"
    string? Email,
    string? PhoneNumber,
    string? Street,
    string? City,
    string? Province,
    string? PostalCode,
    string CustomerType,        // "Retail" | "Wholesale"
    string? Notes
) : IRequest<Guid>;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    private static readonly string[] ValidDocTypes  = ["DNI", "RUC", "CE", "Pasaporte"];
    private static readonly string[] ValidCustTypes = ["Retail", "Wholesale"];

    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().MaximumLength(100)
            .WithMessage("El nombre es requerido (máx 100 caracteres).");

        RuleFor(x => x.LastName)
            .NotEmpty().MaximumLength(100)
            .WithMessage("El apellido es requerido (máx 100 caracteres).");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().MaximumLength(20)
            .WithMessage("El número de documento es requerido.");

        RuleFor(x => x.DocumentType)
            .Must(t => ValidDocTypes.Contains(t))
            .WithMessage("Tipo de documento inválido. Use: DNI, RUC, CE o Pasaporte.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("El email no tiene formato válido.");

        RuleFor(x => x.CustomerType)
            .Must(t => ValidCustTypes.Contains(t))
            .WithMessage("Tipo de cliente inválido. Use 'Retail' o 'Wholesale'.");
    }
}

public class CreateCustomerCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateCustomerCommand, Guid>
{
    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        // Verificar documento único
        var existingDoc = await customerRepository.GetByDocumentNumberAsync(request.DocumentNumber, ct);
        if (existingDoc != null)
            throw new ConflictException($"Ya existe un cliente con el documento '{request.DocumentNumber}'.");

        // Verificar email único si se proporciona
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var existingEmail = await customerRepository.GetByEmailAsync(request.Email, ct);
            if (existingEmail != null)
                throw new ConflictException($"Ya existe un cliente con el email '{request.Email}'.");
        }

        var docType = Enum.Parse<DocumentType>(request.DocumentType);
        var custType = Enum.Parse<CustomerType>(request.CustomerType);

        var customer = Customer.Create(
            request.FirstName,
            request.LastName,
            request.DocumentNumber,
            docType,
            request.Email?.ToLowerInvariant(),
            request.PhoneNumber
        );

        customer.Update(
            request.FirstName, request.LastName,
            request.DocumentNumber, docType,
            request.Email?.ToLowerInvariant(), request.PhoneNumber,
            request.Street, request.City, request.Province, request.PostalCode,
            custType, request.Notes
        );

        unitOfWork.Add(customer);
        await unitOfWork.SaveChangesAsync(ct);
        return customer.Id;
    }
}
