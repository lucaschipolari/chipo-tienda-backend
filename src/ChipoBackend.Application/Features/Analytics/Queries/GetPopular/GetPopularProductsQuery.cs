using ChipoBackend.Domain.Entities.Analytics;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Analytics.Queries.GetPopular;

public record PopularProductDto(Guid ProductId, int Views);

public record GetPopularProductsQuery(int Days = 30) : IRequest<IReadOnlyList<PopularProductDto>>;

public class GetPopularProductsQueryHandler(
    IAnalyticsEventRepository analytics
) : IRequestHandler<GetPopularProductsQuery, IReadOnlyList<PopularProductDto>>
{
    public async Task<IReadOnlyList<PopularProductDto>> Handle(GetPopularProductsQuery request, CancellationToken ct)
    {
        var to = DateTime.UtcNow;
        var from = to.AddDays(-Math.Clamp(request.Days, 1, 365));
        var top = await analytics.TopProductsAsync(AnalyticsEventType.ProductView, from, to, 1000, ct);
        return top.Select(t => new PopularProductDto(t.ProductId, t.Count)).ToList();
    }
}
