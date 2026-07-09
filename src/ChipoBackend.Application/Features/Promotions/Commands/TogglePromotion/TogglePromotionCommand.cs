using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Commands.TogglePromotion;

public record TogglePromotionCommand(Guid Id) : IRequest<Unit>;

public class TogglePromotionCommandHandler(
    IPromotionRepository promotionRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<TogglePromotionCommand, Unit>
{
    public async Task<Unit> Handle(TogglePromotionCommand request, CancellationToken cancellationToken)
    {
        var promotion = await promotionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Promotion", request.Id);

        promotion.Toggle();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
