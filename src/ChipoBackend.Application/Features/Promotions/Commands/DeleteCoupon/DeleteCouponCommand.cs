using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Promotions.Commands.DeleteCoupon;

public record DeleteCouponCommand(Guid Id) : IRequest<Unit>;

public class DeleteCouponCommandHandler : IRequestHandler<DeleteCouponCommand, Unit>
{
    private readonly ICouponRepository _couponRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCouponCommandHandler(ICouponRepository couponRepository, IUnitOfWork unitOfWork)
    {
        _couponRepository = couponRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _couponRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Coupon), request.Id);

        if (coupon.IsDeleted)
            throw new NotFoundException(nameof(Coupon), request.Id);

        coupon.SoftDelete();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
