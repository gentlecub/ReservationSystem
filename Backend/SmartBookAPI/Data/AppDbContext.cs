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
    public DbSet<CalendarConnection> CalendarConnections { get; set; }
    public DbSet<ReservationCalendarEvent> ReservationCalendarEvents { get; set; }
    public DbSet<WaitlistEntry> WaitlistEntries { get; set; }

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

        // ============================================
        // CONFIGURACIÓN DE LA ENTIDAD CALENDAR CONNECTION
        // ============================================
        modelBuilder.Entity<CalendarConnection>(entity =>
        {
            entity.ToTable("CalendarConnections");

            // Relación: Una CalendarConnection pertenece a un User
            entity.HasOne(c => c.User)
                  .WithMany()
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Un usuario solo puede tener una conexión por proveedor
            entity.HasIndex(c => new { c.UserId, c.Provider })
                  .IsUnique();
        });

        // ============================================
        // CONFIGURACIÓN DE LA ENTIDAD RESERVATION CALENDAR EVENT
        // ============================================
        modelBuilder.Entity<ReservationCalendarEvent>(entity =>
        {
            entity.ToTable("ReservationCalendarEvents");

            // Relación: Un ReservationCalendarEvent pertenece a una Reservation
            entity.HasOne(e => e.Reservation)
                  .WithMany()
                  .HasForeignKey(e => e.ReservationId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Relación: Un ReservationCalendarEvent pertenece a una CalendarConnection
            entity.HasOne(e => e.CalendarConnection)
                  .WithMany(c => c.CalendarEvents)
                  .HasForeignKey(e => e.CalendarConnectionId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Índice para buscar eventos por reserva
            entity.HasIndex(e => e.ReservationId);
        });

        // ============================================
        // CONFIGURACIÓN DE LA ENTIDAD WAITLIST ENTRY
        // ============================================
        modelBuilder.Entity<WaitlistEntry>(entity =>
        {
            entity.ToTable("WaitlistEntries");

            // Relación: Una WaitlistEntry pertenece a un User
            entity.HasOne(w => w.User)
                  .WithMany()
                  .HasForeignKey(w => w.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Relación: Una WaitlistEntry pertenece a un Resource
            entity.HasOne(w => w.Resource)
                  .WithMany()
                  .HasForeignKey(w => w.ResourceId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Índice compuesto para verificar entradas duplicadas
            entity.HasIndex(w => new { w.UserId, w.ResourceId, w.PreferredDate, w.Status });

            // Índice para buscar por recurso y fecha
            entity.HasIndex(w => new { w.ResourceId, w.PreferredDate, w.Status });

            // Índice para buscar por usuario
            entity.HasIndex(w => w.UserId);

            // Índice para ordenar por posición
            entity.HasIndex(w => w.Position);
        });
    }
}
