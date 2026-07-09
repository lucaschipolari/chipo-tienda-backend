using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Commands.DeleteDiscount;

public record DeleteDiscountCommand(Guid Id) : IRequest<Unit>;

public class DeleteDiscountCommandHandler(
    IDiscountRepository discountRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteDiscountCommand, Unit>
{
    public async Task<Unit> Handle(DeleteDiscountCommand request, CancellationToken cancellationToken)
    {
        var discount = await discountRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Discount", request.Id);

        discount.SoftDelete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
