namespace ChipoBackend.Application.Features.Users.DTOs;

public record UserDto(Guid Id, string Email, string FirstName, string LastName, string? PhoneNumber, string Status, bool IsEmailConfirmed, DateTime? LastLoginAt, IReadOnlyList<string> Roles, DateTime CreatedAt);

public record UserListItemDto(Guid Id, string Email, string FullName, string Status, IReadOnlyList<string> Roles, DateTime CreatedAt);
