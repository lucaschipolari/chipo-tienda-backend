using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Suppliers.Commands.ChangeSupplierStatus;

public record ChangeSupplierStatusCommand(Guid Id, bool IsActive) : IRequest;

public class ChangeSupplierStatusCommandHandler(
    ISupplierRepository supplierRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ChangeSupplierStatusCommand>
{
    public async Task Handle(ChangeSupplierStatusCommand request, CancellationToken ct)
    {
        var supplier = await supplierRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Proveedor", request.Id);

        if (request.IsActive)
            supplier.Activate();
        else
            supplier.Deactivate();

        await unitOfWork.SaveChangesAsync(ct);
    }
}
