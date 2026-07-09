using ChipoBackend.Application.Common.Models;
using ChipoBackend.Application.Features.Suppliers.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Suppliers.Queries.GetSuppliers;

public record GetSuppliersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    bool? IsActive = null
) : IRequest<PagedResult<SupplierListItemDto>>;

public class GetSuppliersQueryHandler(
    ISupplierRepository supplierRepository
) : IRequestHandler<GetSuppliersQuery, PagedResult<SupplierListItemDto>>
{
    public async Task<PagedResult<SupplierListItemDto>> Handle(GetSuppliersQuery request, CancellationToken ct)
    {
        var (suppliers, total) = await supplierRepository.GetPagedAsync(
            request.Page, request.PageSize, request.Search, request.IsActive, ct);

        var items = suppliers.Select(s => new SupplierListItemDto(
            Id: s.Id,
            CompanyName: s.CompanyName,
            TradeName: s.TradeName,
            TaxId: s.TaxId,
            Email: s.Email,
            Phone: s.Phone,
            City: s.City,
            IsActive: s.IsActive,
            ProductCount: s.Products.Count,
            CreatedAt: s.CreatedAt
        )).ToList();

        return PagedResult<SupplierListItemDto>.Create(items, total, request.Page, request.PageSize);
    }
}
