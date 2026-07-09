using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Customers.Commands.ChangeCustomerStatus;

public record ChangeCustomerStatusCommand(Guid Id, bool IsActive) : IRequest;

public class ChangeCustomerStatusCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ChangeCustomerStatusCommand>
{
    public async Task Handle(ChangeCustomerStatusCommand request, CancellationToken ct)
    {
        var customer = await customerRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Cliente '{request.Id}' no encontrado.");

        if (request.IsActive) customer.Activate();
        else customer.Deactivate();

        await unitOfWork.SaveChangesAsync(ct);
    }
}
