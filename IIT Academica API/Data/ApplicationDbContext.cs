using Microsoft.EntityFrameworkCore;
using IIT_Academica_API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; // Needed for IdentityRole<int> etc.

// FIX 1: Change inheritance to use int (TKey) for all Identity-related tables
public class ApplicationDbContext : IdentityDbContext<
    ApplicationUser,
    IdentityRole<int>, // Role key type
    int,              // User key type
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

    // ADDED: The new central class instance entity
    public DbSet<Subject> Subjects { get; set; }

    public DbSet<CourseMaterial> CourseMaterials { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Must be called first to configure Identity tables
        base.OnModelCreating(modelBuilder);

        // -------------------------------------------------------------
        // 1. TeacherSubject Relationships (Teacher is an int key)
        // -------------------------------------------------------------

        // Configure the one-to-many relationship between ApplicationUser (Teacher) and TeacherSubject
        modelBuilder.Entity<Subject>()
            .HasOne(ts => ts.Teacher) // The TeacherSubject has one Teacher
                                      // NOTE: You must ensure you have added 'public ICollection<TeacherSubject> TaughtTeacherSubjects { get; set; }' to ApplicationUser.cs
            .WithMany(u => u.TaughtSubjects)
            .HasForeignKey(ts => ts.TeacherId) // TeacherId is now int
            .OnDelete(DeleteBehavior.Restrict);

        // Ensure the RegistrationCode is unique for enrollment
        modelBuilder.Entity<Subject>()
            .HasIndex(ts => ts.RegistrationCode)
            .IsUnique();

        // -------------------------------------------------------------
        // 2. Updated Relationships (Now linking to TeacherSubject)
        // -------------------------------------------------------------

        // CourseMaterial now links to TeacherSubject
        modelBuilder.Entity<CourseMaterial>()
            .HasOne(cm => cm.Subject)
            .WithMany(ts => ts.CourseMaterials)
            .HasForeignKey(cm => cm.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Enrollment now links to TeacherSubject
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Subject)
            .WithMany(ts => ts.Enrollments)
            .HasForeignKey(e => e.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // AttendanceRecord now links to TeacherSubject
        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(ar => ar.Subject)
            .WithMany(ts => ts.AttendanceSessions)
            .HasForeignKey(ar => ar.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);


        // -------------------------------------------------------------
        // 3. Enrollment and User Relationships (StudentId is now an int key)
        // -------------------------------------------------------------

        // Enrollment to Student (User)
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(u => u.Enrollments)
            .HasForeignKey(e => e.StudentId) // StudentId is now int
            .OnDelete(DeleteBehavior.Restrict);

        // Other Configuration (Remains the same)
        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Note: All obsolete Space configurations were successfully removed.
    }
}