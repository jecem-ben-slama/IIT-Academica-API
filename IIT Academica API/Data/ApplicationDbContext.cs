using Microsoft.EntityFrameworkCore;
using IIT_Academica_API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
public class ApplicationDbContext : IdentityDbContext<
    ApplicationUser,
    IdentityRole<int>,
    int,
    IdentityUserClaim<int>,
    IdentityUserRole<int>,
    IdentityUserLogin<int>,
    IdentityRoleClaim<int>,
    IdentityUserToken<int>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Subject> Subjects { get; set; }

    public DbSet<CourseMaterial> CourseMaterials { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Subject>()
            .HasOne(ts => ts.Teacher)
            .WithMany(u => u.TaughtSubjects)
            .HasForeignKey(ts => ts.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Subject>()
            .HasIndex(ts => ts.RegistrationCode)
            .IsUnique();


        modelBuilder.Entity<CourseMaterial>()
            .HasOne(cm => cm.Subject)
            .WithMany(ts => ts.CourseMaterials)
            .HasForeignKey(cm => cm.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Subject)
            .WithMany(ts => ts.Enrollments)
            .HasForeignKey(e => e.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(ar => ar.Subject)
            .WithMany(ts => ts.AttendanceSessions)
            .HasForeignKey(ar => ar.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);



        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(u => u.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

    }
}
