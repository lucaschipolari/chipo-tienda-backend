using ChipoBackend.Application.Features.Combos.DTOs;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Combos.Queries.GetCombos;

public record GetCombosQuery(bool OnlyActive) : IRequest<IReadOnlyList<ComboDto>>;

public class GetCombosQueryHandler(
    IComboRepository comboRepository,
    IProductRepository productRepository
) : IRequestHandler<GetCombosQuery, IReadOnlyList<ComboDto>>
{
    public async Task<IReadOnlyList<ComboDto>> Handle(GetCombosQuery request, CancellationToken ct)
    {
        var combos = await comboRepository.GetAllWithItemsAsync(request.OnlyActive, ct);
        var result = new List<ComboDto>();
        foreach (var c in combos)
            result.Add(await ComboMapper.ToDtoAsync(c, productRepository, ct));
        return result;
    }
}
