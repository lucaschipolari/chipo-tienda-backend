using ChipoBackend.Domain.Common;

namespace ChipoBackend.Domain.Entities.Expenses;

public class ExpenseCategory : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Color { get; private set; } = "#6B7280";
    public bool IsActive { get; private set; } = true;

    private ExpenseCategory() { }

    public static ExpenseCategory Create(string name, string? description, string? color, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la categoría es requerido.", nameof(name));

        return new ExpenseCategory
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            Color = string.IsNullOrWhiteSpace(color) ? "#6B7280" : color.Trim(),
            IsActive = true,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description, string? color)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la categoría es requerido.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        Color = string.IsNullOrWhiteSpace(color) ? "#6B7280" : color.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
