using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Sales.Commands.DeleteSale;

public record DeleteSaleCommand(Guid Id) : IRequest;

public class DeleteSaleCommandHandler(
    ISaleRepository saleRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteSaleCommand>
{
    public async Task Handle(DeleteSaleCommand request, CancellationToken ct)
    {
        var sale = await saleRepository.GetWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException($"Venta '{request.Id}' no encontrada.");

        saleRepository.Remove(sale);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
