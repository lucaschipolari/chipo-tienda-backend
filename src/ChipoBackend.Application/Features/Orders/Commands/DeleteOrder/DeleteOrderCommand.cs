using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Orders.Commands.DeleteOrder;

/// <summary>
/// Elimina físicamente un pedido (y sus ítems por cascada). Reservado a
/// administradores: permite limpiar pedidos de prueba o cargas erróneas.
/// No restaura stock — usar Cancelar si el pedido ya descontó inventario.
/// </summary>
public record DeleteOrderCommand(Guid Id) : IRequest;

public class DeleteOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteOrderCommand>
{
    public async Task Handle(DeleteOrderCommand request, CancellationToken ct)
    {
        var order = await orderRepository.GetWithItemsAsync(request.Id, ct)
            ?? throw new NotFoundException($"Pedido '{request.Id}' no encontrado.");

        orderRepository.Remove(order);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
