using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Features.Combos.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Combos.Queries.GetComboById;

public record GetComboByIdQuery(Guid Id) : IRequest<ComboDto>;

public class GetComboByIdQueryHandler(
    IComboRepository comboRepository,
    IProductRepository productRepository
) : IRequestHandler<GetComboByIdQuery, ComboDto>
{
    public async Task<ComboDto> Handle(GetComboByIdQuery request, CancellationToken ct)
    {
        var combo = await comboRepository.GetWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException("Combo", request.Id);
        return await ComboMapper.ToDtoAsync(combo, productRepository, ct);
    }
}
