using ChipoBackend.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChipoBackend.Infrastructure.Persistence;

/// <summary>
/// Inicializa la base de datos con datos mínimos necesarios para arrancar la aplicación.
/// Usa SQL directo para evitar conflictos con el AuditSaveChangesInterceptor durante el startup.
/// Idempotente — se puede ejecutar múltiples veces sin efectos secundarios.
/// </summary>
public static class DbInitializer
{
    // ── Roles del sistema ──────────────────────────────────────────────────────

    private static readonly (string Name, string Description, bool IsSystem)[] SystemRoles =
    [
        ("SuperAdmin", "Acceso total al sistema",           true),
        ("Admin",      "Administrador general",             true),
        ("Supervisor", "Supervisión de ventas y pedidos",   true),
        ("Vendedor",   "Creación y gestión de ventas",      true),
        ("Almacen",    "Gestión de inventario y stock",     true),
        ("Finance",    "Acceso a módulos financieros",      true),
        ("Customer",   "Cliente de la tienda online",       true),
    ];

    // ── SuperAdmin por defecto ─────────────────────────────────────────────────

    // En producción se sobreescriben con las variables de entorno ADMIN_EMAIL / ADMIN_PASSWORD
    private static readonly string DefaultEmail    = Environment.GetEnvironmentVariable("ADMIN_EMAIL")    ?? "admin@chipo.com";
    private static readonly string DefaultPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin1234!";
    private const string DefaultFirstName = "Super";
    private const string DefaultLastName  = "Admin";

    // ─────────────────────────────────────────────────────────────────────────

    public static async Task SeedAsync(AppDbContext db, ILogger logger)
    {
        try
        {
            // 1. Aplicar migraciones pendientes
            var pending = await db.Database.GetPendingMigrationsAsync();
            if (pending.Any())
            {
                logger.LogInformation("Aplicando {Count} migraciones pendientes...", pending.Count());
                await db.Database.MigrateAsync();
            }

            // 2. Seed con SQL directo — bypass total del interceptor de auditoría
            await SeedRolesAsync(db, logger);
            await SeedSuperAdminAsync(db, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al inicializar la base de datos.");
            throw;
        }
    }

    // ── Roles ─────────────────────────────────────────────────────────────────

    private static async Task SeedRolesAsync(AppDbContext db, ILogger logger)
    {
        // Obtener nombres existentes con SQL directo
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();

        try
        {
            foreach (var (name, description, isSystem) in SystemRoles)
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO auth.roles (""Id"", ""Name"", ""Description"", ""IsSystem"")
                    VALUES (@id, @name, @desc, @isSystem)
                    ON CONFLICT (""Name"") DO NOTHING";

                AddParam(cmd, "@id",       Guid.NewGuid());
                AddParam(cmd, "@name",     name);
                AddParam(cmd, "@desc",     description);
                AddParam(cmd, "@isSystem", isSystem);

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows > 0)
                    logger.LogInformation("Rol '{Role}' creado.", name);
                else
                    logger.LogDebug("Rol '{Role}' ya existe — omitido.", name);
            }
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    // ── SuperAdmin ────────────────────────────────────────────────────────────

    private static async Task SeedSuperAdminAsync(AppDbContext db, ILogger logger)
    {
        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();

        try
        {
            // Verificar si ya existe
            await using var checkCmd = conn.CreateCommand();
            checkCmd.CommandText = @"SELECT COUNT(1) FROM auth.users WHERE email = @email";
            AddParam(checkCmd, "@email", DefaultEmail.ToLowerInvariant());
            var count = (long)(await checkCmd.ExecuteScalarAsync() ?? 0L);

            if (count > 0)
            {
                logger.LogDebug("Usuario SuperAdmin ya existe — omitido.");
                return;
            }

            // Obtener Id del rol SuperAdmin
            await using var roleCmd = conn.CreateCommand();
            roleCmd.CommandText = @"SELECT ""Id"" FROM auth.roles WHERE ""Name"" = 'SuperAdmin' LIMIT 1";
            var roleIdObj = await roleCmd.ExecuteScalarAsync();

            if (roleIdObj is null)
            {
                logger.LogWarning("Rol SuperAdmin no encontrado. Abortando creación de usuario.");
                return;
            }

            var roleId  = (Guid)roleIdObj;
            var userId  = Guid.NewGuid();
            var now     = DateTime.UtcNow;
            var pwdHash = new PasswordService().Hash(DefaultPassword);

            // Insertar usuario
            await using var userCmd = conn.CreateCommand();
            userCmd.CommandText = @"
                INSERT INTO auth.users (
                    ""Id"", email, ""PasswordHash"", ""FirstName"", ""LastName"",
                    ""Status"", ""IsEmailConfirmed"", ""CreatedAt"", ""UpdatedAt"")
                VALUES (
                    @id, @email, @pwd, @firstName, @lastName,
                    'Active', false, @now, @now)";

            AddParam(userCmd, "@id",        userId);
            AddParam(userCmd, "@email",     DefaultEmail.ToLowerInvariant());
            AddParam(userCmd, "@pwd",       pwdHash);
            AddParam(userCmd, "@firstName", DefaultFirstName);
            AddParam(userCmd, "@lastName",  DefaultLastName);
            AddParam(userCmd, "@now",       now);

            await userCmd.ExecuteNonQueryAsync();

            // Insertar relación user_role
            await using var urCmd = conn.CreateCommand();
            urCmd.CommandText = @"
                INSERT INTO auth.user_roles (""UserId"", ""RoleId"")
                VALUES (@userId, @roleId)
                ON CONFLICT DO NOTHING";

            AddParam(urCmd, "@userId", userId);
            AddParam(urCmd, "@roleId", roleId);
            await urCmd.ExecuteNonQueryAsync();

            logger.LogInformation(
                "Usuario SuperAdmin creado — Email: {Email} | Password: {Password}",
                DefaultEmail, DefaultPassword);
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static void AddParam(System.Data.Common.DbCommand cmd, string name, object? value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(p);
    }
}
