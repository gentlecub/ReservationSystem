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
// ============================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey no está configurado en appsettings.json");

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
// CONFIGURACIÓN DE CORS
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://localhost:3000",
                "https://localhost:5173"
              )
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

// Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS (debe ir antes de Authentication)
app.UseCors("AllowReactApp");

// Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();

// Mapear Controllers
app.MapControllers();

// ============================================
// SEED DATA INICIAL
// ============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await DbSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al ejecutar el seed de datos");
    }
}

app.Run();
