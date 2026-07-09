using ChipoBackend.Application.Common.Exceptions;
using ChipoBackend.Application.Common.Interfaces;
using ChipoBackend.Domain.Entities.Users;
using ChipoBackend.Domain.Interfaces;
using ChipoBackend.Domain.Interfaces.Repositories;
using MediatR;

namespace ChipoBackend.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordService passwordService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        // ExistsAsync con u.Email.Value no puede traducirse a SQL con HasConversion.
        // GetByEmailAsync usa u.Email == EmailAddress.Of(email), que sí se traduce.
        if (await userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), ct) != null)
            throw new ConflictException($"El email '{request.Email}' ya está registrado.");

        var passwordHash = passwordService.Hash(request.Password);
        var user = User.Create(request.Email, passwordHash, request.FirstName, request.LastName, request.PhoneNumber);

        var customerRole = await roleRepository.GetByNameAsync("Customer", ct);
        if (customerRole != null)
            user.AssignRole(customerRole.Id);

        await userRepository.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new RegisterResponse(user.Id, user.Email.Value, user.FullName);
    }
}
