using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Users;

public class Permission : BaseEntity
{
    public string Resource { get; private set; } = null!;
    public string Action { get; private set; } = null!;
    public string? Description { get; private set; }

    public string FullName => $"{Resource}:{Action}";

    private Permission() { }

    public static Permission Create(string resource, string action, string? description = null)
    {
        return new Permission { Resource = resource, Action = action, Description = description };
    }
}
