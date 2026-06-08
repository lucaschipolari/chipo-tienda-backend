using ChipoBackend.Domain.Common;
using ChipoBackend.Domain.Exceptions;
using ChipoBackend.Domain.ValueObjects;

namespace ChipoBackend.Domain.Entities.Users;

public class User : AuditableEntity
{
    public EmailAddress Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }
    public UserStatus Status { get; private set; } = UserStatus.Active;
    public bool IsEmailConfirmed { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private readonly List<UserRole> _roles = [];
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { }

    public static User Create(string email, string passwordHash, string firstName, string lastName, string? phoneNumber = null)
    {
        return new User
        {
            Email = EmailAddress.Of(email),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public string FullName => $"{FirstName} {LastName}";

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
        RevokeAllRefreshTokens("password_changed");
    }

    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        if (Status == UserStatus.Suspended)
            throw new BusinessRuleException("UserAlreadySuspended", "El usuario ya está suspendido.");
        Status = UserStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
        RevokeAllRefreshTokens("account_suspended");
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Block()
    {
        Status = UserStatus.Blocked;
        UpdatedAt = DateTime.UtcNow;
        RevokeAllRefreshTokens("account_blocked");
    }

    public void AssignRole(Guid roleId)
    {
        if (_roles.Any(r => r.RoleId == roleId))
            return;
        _roles.Add(new UserRole { UserId = Id, RoleId = roleId });
    }

    public void RemoveRole(Guid roleId) =>
        _roles.RemoveAll(r => r.RoleId == roleId);

    public RefreshToken AddRefreshToken(string tokenHash, DateTime expiresAt, string? createdByIp)
    {
        var token = RefreshToken.Create(Id, tokenHash, expiresAt, createdByIp);
        _refreshTokens.Add(token);
        return token;
    }

    public void RevokeRefreshToken(string tokenHash, string reason)
    {
        var token = _refreshTokens.FirstOrDefault(t => t.TokenHash == tokenHash);
        token?.Revoke(reason);
    }

    private void RevokeAllRefreshTokens(string reason)
    {
        foreach (var token in _refreshTokens.Where(t => !t.IsRevoked))
            token.Revoke(reason);
    }

    public bool IsActive => Status == UserStatus.Active;
}

public enum UserStatus { Active, Suspended, Blocked }
