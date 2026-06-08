namespace ChipoBackend.Application.Features.Roles.DTOs;

public record RoleDto(Guid Id, string Name, string? Description, bool IsSystem, IReadOnlyList<PermissionDto> Permissions);
public record RoleListItemDto(Guid Id, string Name, string? Description, bool IsSystem, int PermissionCount);
public record PermissionDto(Guid Id, string Resource, string Action, string FullName, string? Description);
