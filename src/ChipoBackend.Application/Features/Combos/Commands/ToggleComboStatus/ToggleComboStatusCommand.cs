using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Combos.Commands.ToggleComboStatus;

public record ToggleComboStatusCommand(Guid Id, bool IsActive) : IRequest;

public class ToggleComboStatusCommandHandler(
    IComboRepository comboRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ToggleComboStatusCommand>
{
    public async Task Handle(ToggleComboStatusCommand request, CancellationToken ct)
    {
        var combo = await comboRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Combo", request.Id);
        combo.SetActive(request.IsActive);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
