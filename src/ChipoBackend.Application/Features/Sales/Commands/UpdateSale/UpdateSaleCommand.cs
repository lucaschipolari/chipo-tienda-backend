using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace ChipoBackend.Application.Features.Sales.Commands.UpdateSale;

/// <summary>
/// Edita datos de una venta ya registrada: fecha, método de pago, notas y
/// nombre de cliente. No modifica los ítems ni el stock (para eso, borrar y
/// volver a crear la venta).
/// </summary>
public record UpdateSaleCommand(
    Guid Id,
    string PaymentMethod,
    string? Notes,
    string? CustomerName,
    DateTime? SaleDate
) : IRequest;

public class UpdateSaleCommandValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.PaymentMethod).NotEmpty().WithMessage("El método de pago es requerido.");
    }
}

public class UpdateSaleCommandHandler(
    ISaleRepository saleRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateSaleCommand>
{
    public async Task Handle(UpdateSaleCommand request, CancellationToken ct)
    {
        var sale = await saleRepository.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException("Venta", request.Id);

        var saleDate = request.SaleDate.HasValue
            ? DateTime.SpecifyKind(request.SaleDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        sale.UpdateInfo(request.PaymentMethod, request.Notes, request.CustomerName, saleDate);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
