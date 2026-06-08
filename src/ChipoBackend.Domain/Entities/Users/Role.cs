using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Users;

public class Role : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; }

    private readonly List<RolePermission> _permissions = [];
    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    private Role() { }

    public static Role Create(string name, string? description = null, bool isSystem = false)
    {
        return new Role { Name = name, Description = description, IsSystem = isSystem };
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void AssignPermission(Guid permissionId)
    {
        if (_permissions.Any(p => p.PermissionId == permissionId)) return;
        _permissions.Add(new RolePermission { RoleId = Id, PermissionId = permissionId });
    }

    public void RemovePermission(Guid permissionId) =>
        _permissions.RemoveAll(p => p.PermissionId == permissionId);

    public void SetPermissions(IEnumerable<Guid> permissionIds)
    {
        _permissions.Clear();
        foreach (var pid in permissionIds)
            _permissions.Add(new RolePermission { RoleId = Id, PermissionId = pid });
    }
}
