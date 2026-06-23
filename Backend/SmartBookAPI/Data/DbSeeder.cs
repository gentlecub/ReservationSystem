using Microsoft.EntityFrameworkCore;
using SmartBookAPI.Models;

namespace SmartBookAPI.Data;

/// <summary>
/// Clase para sembrar datos iniciales en la base de datos
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {

        // Seed de Roles (si no existen)
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new Role { RoleName = "Admin" },
                new Role { RoleName = "Client" }
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // Seed de Usuario Admin (si no existe)
        if (!await context.Users.AnyAsync(u => u.Email == "admin@smartbook.com"))
        {
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");

            if (adminRole != null)
            {
                var adminUser = new User
                {
                    FullName = "Administrador",
                    Email = "admin@smartbook.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    RoleId = adminRole.RoleId,
                    CreatedAt = DateTime.UtcNow
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }
        }

        // Seed de Resources de ejemplo (si no existen)
        if (!await context.Resources.AnyAsync())
        {
            var resources = new List<Resource>
            {
                new Resource
                {
                    Name = "Cancha de Fútbol",
                    Description = "Cancha de fútbol 5 con césped sintético",
                    Location = "Zona deportiva - Sector A",
                    IsActive = true
                },
                new Resource
                {
                    Name = "Sala de Reuniones A",
                    Description = "Sala para 10 personas con proyector y pizarra",
                    Location = "Edificio Principal - Piso 2",
                    IsActive = true
                }
            };

            await context.Resources.AddRangeAsync(resources);
            await context.SaveChangesAsync();
        }
    }
}
