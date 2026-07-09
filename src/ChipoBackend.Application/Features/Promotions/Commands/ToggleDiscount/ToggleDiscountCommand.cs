using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Commands.ToggleDiscount;

public record ToggleDiscountCommand(Guid Id) : IRequest<Unit>;

public class ToggleDiscountCommandHandler(
    IDiscountRepository discountRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ToggleDiscountCommand, Unit>
{
    public async Task<Unit> Handle(ToggleDiscountCommand request, CancellationToken cancellationToken)
    {
        var discount = await discountRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Discount", request.Id);

        discount.Toggle();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
