using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Combos.Commands.DeleteCombo;

public record DeleteComboCommand(Guid Id) : IRequest;

public class DeleteComboCommandHandler(
    IComboRepository comboRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteComboCommand>
{
    public async Task Handle(DeleteComboCommand request, CancellationToken ct)
    {
        var combo = await comboRepository.GetWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException("Combo", request.Id);
        comboRepository.Remove(combo);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
