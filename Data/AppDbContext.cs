using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TfgApi.Models;
using DayOfWeek = TfgApi.Models.DayOfWeek;

namespace TfgApi.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<DayOfWeek> DayOfWeeks { get; set; }
        public DbSet<Routine> Routines { get; set; }
        public DbSet<RoutineDay> RoutineDays { get; set; }
        public DbSet<RoutineExercise> RoutineExercises { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<Routine>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Exercise>(entity =>
            {
                entity.HasIndex(e => e.ExternalApiId).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.GifUrl).HasMaxLength(500);
            });

            builder.Entity<DayOfWeek>(entity =>
            {
                entity.HasIndex(d => d.Name).IsUnique();
                entity.Property(d => d.Name).IsRequired().HasMaxLength(20);
                entity.HasData(
                    new DayOfWeek { Id = 1, Name = "Monday", Order = 1 },
                    new DayOfWeek { Id = 2, Name = "Tuesday", Order = 2 },
                    new DayOfWeek { Id = 3, Name = "Wednesday", Order = 3 },
                    new DayOfWeek { Id = 4, Name = "Thursday", Order = 4 },
                    new DayOfWeek { Id = 5, Name = "Friday", Order = 5 },
                    new DayOfWeek { Id = 6, Name = "Saturday", Order = 6 },
                    new DayOfWeek { Id = 7, Name = "Sunday", Order = 7 }
                );
            });

            builder.Entity<Routine>(entity =>
            {
                entity.Property(r => r.Name).IsRequired().HasMaxLength(200);
                entity.Property(r => r.Description).HasMaxLength(1000);
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("now()");

                entity.HasOne(r => r.User)
                    .WithMany(u => u.Routines)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<RoutineDay>(entity =>
            {
                entity.HasKey(rd => rd.Id);

                entity.HasIndex(rd => new { rd.RoutineId, rd.DayOfWeekId }).IsUnique();

                entity.HasOne(rd => rd.Routine)
                    .WithMany(r => r.RoutineDays)
                    .HasForeignKey(rd => rd.RoutineId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rd => rd.DayOfWeek)
                    .WithMany()
                    .HasForeignKey(rd => rd.DayOfWeekId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<RoutineExercise>(entity =>
            {
                entity.HasKey(re => re.Id);

                entity.HasOne(re => re.Routine)
                    .WithMany(r => r.RoutineExercises)
                    .HasForeignKey(re => re.RoutineId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(re => re.Exercise)
                    .WithMany()
                    .HasForeignKey(re => re.ExerciseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(re => re.Sets).HasDefaultValue(1);
                entity.Property(re => re.Reps).HasDefaultValue("10");
                entity.Property(re => re.RestTimeSeconds).HasDefaultValue(60);
            });
        }
    }
}
