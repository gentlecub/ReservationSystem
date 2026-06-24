using Microsoft.EntityFrameworkCore;
using SmartBookAPI.Models;

namespace SmartBookAPI.Data;

/// <summary>
/// Contexto de base de datos principal de la aplicación.
/// Configura las entidades, relaciones y restricciones.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets: Representan las tablas en la base de datos
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<NotificationLog> NotificationLogs { get; set; }

    /// <summary>
    /// Configura el modelo de datos usando Fluent API.
    /// Aquí definimos índices, restricciones y relaciones.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================
        // CONFIGURACIÓN DE LA ENTIDAD ROLE
        // ============================================
        modelBuilder.Entity<Role>(entity =>
        {
            // Nombre de la tabla
            entity.ToTable("Roles");

            // El nombre del rol debe ser único
            entity.HasIndex(r => r.RoleName)
                  .IsUnique();
        });

        // ============================================
        // CONFIGURACIÓN DE LA ENTIDAD USER
        // ============================================
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            // El email debe ser único
            entity.HasIndex(u => u.Email)
                  .IsUnique();

            // Relación: Un User pertenece a un Role
            // Un Role puede tener muchos Users
            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleId)
                  .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar rol si tiene usuarios
        });

        // ============================================
        // CONFIGURACIÓN DE LA ENTIDAD RESOURCE
        // ============================================
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.ToTable("Resources");

            // Índice para búsquedas por nombre
            entity.HasIndex(r => r.Name);
        });

        // ============================================
        // CONFIGURACIÓN DE LA ENTIDAD RESERVATION
        // ============================================
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.ToTable("Reservations");

            // Relación: Una Reservation pertenece a un User
            entity.HasOne(r => r.User)
                  .WithMany(u => u.Reservations)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade); // Si se elimina usuario, eliminar sus reservas

            // Relación: Una Reservation pertenece a un Resource
            entity.HasOne(r => r.Resource)
                  .WithMany(res => res.Reservations)
                  .HasForeignKey(r => r.ResourceId)
                  .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar recurso con reservas

            // Índice compuesto para verificar disponibilidad rápidamente
            // Útil para consultas de: ¿Está disponible este recurso en esta fecha?
            entity.HasIndex(r => new { r.ResourceId, r.Date, r.StartTime, r.EndTime });

            // Índice para buscar reservas por usuario
            entity.HasIndex(r => r.UserId);

            // Índice para buscar reservas por estado
            entity.HasIndex(r => r.Status);
        });

        // ============================================
        // CONFIGURACIÓN DE LA ENTIDAD NOTIFICATION LOG
        // ============================================
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.ToTable("NotificationLogs");

            // Relación: Un NotificationLog pertenece a una Reservation (opcional)
            entity.HasOne(n => n.Reservation)
                  .WithMany()
                  .HasForeignKey(n => n.ReservationId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Relación: Un NotificationLog pertenece a un User
            entity.HasOne(n => n.User)
                  .WithMany()
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Índice para buscar notificaciones por reserva
            entity.HasIndex(n => n.ReservationId);

            // Índice para buscar notificaciones por usuario
            entity.HasIndex(n => n.UserId);

            // Índice para buscar notificaciones por tipo
            entity.HasIndex(n => n.Type);
        });
    }
}
