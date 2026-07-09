using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Entities.Customers;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string DocumentNumber,
    string DocumentType,
    string? Email,
    string? PhoneNumber,
    string? Street,
    string? City,
    string? Province,
    string? PostalCode,
    string CustomerType,
    string? Notes
) : IRequest;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    private static readonly string[] ValidDocTypes  = ["DNI", "RUC", "CE", "Pasaporte"];
    private static readonly string[] ValidCustTypes = ["Retail", "Wholesale"];

    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DocumentNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DocumentType).Must(t => ValidDocTypes.Contains(t));
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.CustomerType).Must(t => ValidCustTypes.Contains(t));
    }
}

public class UpdateCustomerCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateCustomerCommand>
{
    public async Task Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        var customer = await customerRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Cliente '{request.Id}' no encontrado.");

        // Verificar documento único (excluyendo el cliente actual)
        var existingDoc = await customerRepository.GetByDocumentNumberAsync(request.DocumentNumber, ct);
        if (existingDoc != null && existingDoc.Id != request.Id)
            throw new ConflictException($"El documento '{request.DocumentNumber}' ya está en uso por otro cliente.");

        // Verificar email único (excluyendo el cliente actual)
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var existing = await customerRepository.GetByEmailAsync(request.Email, ct);
            if (existing != null && existing.Id != request.Id)
                throw new ConflictException($"El email '{request.Email}' ya está en uso por otro cliente.");
        }

        var docType = Enum.Parse<DocumentType>(request.DocumentType);
        var custType = Enum.Parse<CustomerType>(request.CustomerType);

        customer.Update(
            request.FirstName, request.LastName,
            request.DocumentNumber, docType,
            request.Email?.ToLowerInvariant(), request.PhoneNumber,
            request.Street, request.City, request.Province, request.PostalCode,
            custType, request.Notes
        );

        await unitOfWork.SaveChangesAsync(ct);
    }
}
