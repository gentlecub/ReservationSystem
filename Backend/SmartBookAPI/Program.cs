using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartBookAPI.Data;
using SmartBookAPI.Middleware;
using SmartBookAPI.Repositories.Implementations;
using SmartBookAPI.Repositories.Interfaces;
using SmartBookAPI.Services.Implementations;
using SmartBookAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// CONFIGURACIÓN DE ENTITY FRAMEWORK CORE
// Produccion = PostgreSQL | Desarrollo = SQL Server
// ============================================
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsProduction())
    {
        // PostgreSQL en produccion (Railway provee DATABASE_URL automaticamente)
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? throw new InvalidOperationException("DATABASE_URL no está configurada en las variables de entorno");

        // Convertir formato URL de Railway a formato Npgsql
        // Railway: postgresql://usuario:password@host:puerto/database
        // Npgsql:  Host=host;Port=puerto;Database=database;Username=usuario;Password=password
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";

        options.UseNpgsql(connectionString);
    }
    else
    {
        // SQL Server en desarrollo (local)
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection no está configurada en appsettings.Development.json");
        options.UseSqlServer(connectionString);
    }
});

// ============================================
// REGISTRO DE REPOSITORIOS (Dependency Injection)
// ============================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

// ============================================
// REGISTRO DE SERVICIOS (Dependency Injection)
// ============================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

// ============================================
// CONFIGURACIÓN DE JWT AUTHENTICATION
// ============================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
// En produccion, JWT_SECRET viene de variable de entorno; en desarrollo, de appsettings
var secretKey = builder.Environment.IsProduction()
    ? Environment.GetEnvironmentVariable("JWT_SECRET")
        ?? throw new InvalidOperationException("JWT_SECRET no está configurada en las variables de entorno")
    : jwtSettings["SecretKey"]
        ?? throw new InvalidOperationException("JWT SecretKey no está configurado en appsettings.Development.json");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // Sin tolerancia de tiempo
    };
});

builder.Services.AddAuthorization();

// ============================================
// CONFIGURACIÓN DE CORS (Dinamico para produccion)
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000", "http://localhost:5173", "https://localhost:5173" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ============================================
// CONFIGURACIÓN DE CONTROLLERS
// ============================================
builder.Services.AddControllers();

// ============================================
// CONFIGURACIÓN DE SWAGGER CON JWT
// ============================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartBook API",
        Version = "v1",
        Description = "API REST para sistema de reservas de recursos"
    });

    // Configuración para usar JWT en Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa tu token JWT. Ejemplo: eyJhbGciOiJIUzI1NiIsInR5..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================
// CONFIGURACIÓN DE PUERTO PARA RAILWAY
// ============================================
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// ============================================
// MIDDLEWARE PIPELINE
// ============================================

// Middleware global de manejo de errores (debe ser el primero)
app.UseMiddleware<ErrorHandlingMiddleware>();

// HSTS y HTTPS Redirection
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

// Swagger habilitado en todos los ambientes
app.UseSwagger();
app.UseSwaggerUI();

// CORS (debe ir antes de Authentication)
app.UseCors("AllowReactApp");

// Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();

// Mapear Controllers
app.MapControllers();

// ============================================
// MIGRACIONES Y SEED DATA INICIAL
// ============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        if (app.Environment.IsProduction())
        {
            // PostgreSQL: crear tablas desde el modelo (sin migraciones de SQL Server)
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            // SQL Server: usar migraciones
            await context.Database.MigrateAsync();
        }

        // Seed de datos iniciales
        await DbSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al ejecutar el seed de datos");
    }
}

app.Run();
