using ChipoBackend.API.Middlewares;
using ChipoBackend.API.ModelBinders;
using ChipoBackend.Application;
using ChipoBackend.Infrastructure;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/chipo-.log", rollingInterval: RollingInterval.Day));

// Application & Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// API
builder.Services.AddControllers(options =>
{
    // Npgsql requiere DateTime con Kind=Utc para columnas timestamptz
    options.ModelBinderProviders.Insert(0, new UtcDateTimeModelBinderProvider());
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
});
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Chipo Backend API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa tu JWT token."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            []
        }
    });
});

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageProducts", policy => policy.RequireRole("Admin", "SuperAdmin"));
    options.AddPolicy("CanViewReports", policy => policy.RequireRole("Admin", "SuperAdmin", "Supervisor"));
    options.AddPolicy("CanManageOrders", policy => policy.RequireRole("Admin", "SuperAdmin", "Supervisor"));
    options.AddPolicy("CanManageInventory", policy => policy.RequireRole("Admin", "SuperAdmin", "Almacen"));
    options.AddPolicy("CanCreateSale", policy => policy.RequireRole("Admin", "SuperAdmin", "Supervisor", "Vendedor"));
    options.AddPolicy("CanViewFinancials", policy => policy.RequireRole("Admin", "SuperAdmin", "Finance"));
    options.AddPolicy("IsAdminOrHigher", policy => policy.RequireRole("Admin", "SuperAdmin"));
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chipo Backend API v1"));
}

app.UseSerilogRequestLogging();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── Seed de base de datos ──────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<ChipoBackend.Infrastructure.Persistence.AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await ChipoBackend.Infrastructure.Persistence.DbInitializer.SeedAsync(db, logger);
}

app.Run();
