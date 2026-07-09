using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Suppliers.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Suppliers.Queries.GetSupplierById;

public record GetSupplierByIdQuery(Guid Id) : IRequest<SupplierDto>;

public class GetSupplierByIdQueryHandler(
    ISupplierRepository supplierRepository
) : IRequestHandler<GetSupplierByIdQuery, SupplierDto>
{
    public async Task<SupplierDto> Handle(GetSupplierByIdQuery request, CancellationToken ct)
    {
        var supplier = await supplierRepository.GetWithContactsAsync(request.Id, ct)
            ?? throw new NotFoundException("Proveedor", request.Id);

        var contacts = supplier.Contacts.Select(c => new SupplierContactDto(
            Id: c.Id,
            Name: c.Name,
            JobTitle: c.JobTitle,
            Email: c.Email,
            Phone: c.Phone,
            IsPrimary: c.IsPrimary
        )).ToList();

        return new SupplierDto(
            Id: supplier.Id,
            CompanyName: supplier.CompanyName,
            TradeName: supplier.TradeName,
            TaxId: supplier.TaxId,
            Email: supplier.Email,
            Phone: supplier.Phone,
            Website: supplier.Website,
            City: supplier.City,
            Province: supplier.Province,
            Country: supplier.Country,
            PaymentTerms: supplier.PaymentTerms,
            IsActive: supplier.IsActive,
            Notes: supplier.Notes,
            Contacts: contacts,
            CreatedAt: supplier.CreatedAt,
            UpdatedAt: supplier.UpdatedAt
        );
    }
}
