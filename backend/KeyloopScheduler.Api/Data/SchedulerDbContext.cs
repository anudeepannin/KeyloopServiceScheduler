
using KeyloopScheduler.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace KeyloopScheduler.Api.Data;

public sealed class SchedulerDbContext : DbContext
{
    public SchedulerDbContext(
        DbContextOptions<SchedulerDbContext> options)
        : base(options)
    {
    }

    // Database tables
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Dealership> Dealerships => Set<Dealership>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<Technician> Technicians => Set<Technician>();
    public DbSet<TechnicianSkill> TechnicianSkills => Set<TechnicianSkill>();
    public DbSet<ServiceBay> ServiceBays => Set<ServiceBay>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(c => c.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(c => c.Email)
                .HasMaxLength(254)
                .IsRequired();

            entity.Property(c => c.Phone)
                .HasMaxLength(30);
        });

        // Vehicle configuration
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.Property(v => v.VIN)
                .HasMaxLength(17)
                .IsRequired();

            entity.HasIndex(v => v.VIN)
                .IsUnique();

            entity.Property(v => v.Make)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(v => v.Model)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasOne(v => v.Customer)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Dealership configuration
        modelBuilder.Entity<Dealership>(entity =>
        {
            entity.Property(d => d.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(d => d.Address)
                .HasMaxLength(300)
                .IsRequired();

            entity.Property(d => d.TimeZoneId)
                .HasMaxLength(100)
                .IsRequired();
        });

        // Service type configuration
        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.Property(s => s.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(s => s.RequiredSkill)
                .HasMaxLength(100)
                .IsRequired();

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_ServiceTypes_DurationMinutes",
                "[DurationMinutes] > 0"));
        });

        // Technician configuration
        modelBuilder.Entity<Technician>(entity =>
        {
            entity.Property(t => t.Name)
                .HasMaxLength(150)
                .IsRequired();

            entity.HasOne(t => t.Dealership)
                .WithMany(d => d.Technicians)
                .HasForeignKey(t => t.DealershipId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Technician skill configuration
        modelBuilder.Entity<TechnicianSkill>(entity =>
        {
            entity.Property(s => s.SkillName)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(s => new
            {
                s.TechnicianId,
                s.SkillName
            })
                .IsUnique();

            entity.HasOne(s => s.Technician)
                .WithMany(t => t.Skills)
                .HasForeignKey(s => s.TechnicianId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Service bay configuration
        modelBuilder.Entity<ServiceBay>(entity =>
        {
            entity.Property(b => b.BayName)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasOne(b => b.Dealership)
                .WithMany(d => d.ServiceBays)
                .HasForeignKey(b => b.DealershipId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Appointment configuration
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.Property(a => a.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Appointments_ValidTimeRange",
                "[EndTimeUtc] > [StartTimeUtc]"));

            // Appointment conflict lookup indexes
            entity.HasIndex(a => new
            {
                a.TechnicianId,
                a.StartTimeUtc,
                a.EndTimeUtc
            });

            entity.HasIndex(a => new
            {
                a.ServiceBayId,
                a.StartTimeUtc,
                a.EndTimeUtc
            });

            // Explicitly configure appointment foreign keys.
            // Prevent SQL Server multiple cascade-delete paths.
            entity.HasOne(a => a.Customer)
                .WithMany(c => c.Appointments)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(a => a.Vehicle)
                .WithMany(v => v.Appointments)
                .HasForeignKey(a => a.VehicleId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(a => a.Dealership)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DealershipId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(a => a.ServiceType)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ServiceTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(a => a.Technician)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TechnicianId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(a => a.ServiceBay)
                .WithMany(b => b.Appointments)
                .HasForeignKey(a => a.ServiceBayId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
