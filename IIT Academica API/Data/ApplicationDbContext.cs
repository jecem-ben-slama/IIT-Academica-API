using Microsoft.EntityFrameworkCore;
using IIT_Academica_API.Entities; // Ensure this matches your Entities namespace

namespace IIT_Academica_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor required for Dependency Injection
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define a DbSet for each entity (this creates your tables)
        public DbSet<User> Users { get; set; }
        public DbSet<TeacherSubject> TeacherSubjects { get; set; }
        public DbSet<CourseMaterial> CourseMaterials { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. TEACHER-SUBJECT Relationship: Block deletion of a User if they teach a subject.
            modelBuilder.Entity<TeacherSubject>()
                .HasOne(ts => ts.Teacher) // The subject has one Teacher
                .WithMany(u => u.TaughtSubjects) // The User can have many TaughtSubjects
                .HasForeignKey(ts => ts.TeacherId)
                .OnDelete(DeleteBehavior.Restrict); // <-- BLOCKS DELETION!

            // 2. STUDENT-ENROLLMENT Relationship: Block deletion of a User if they have an active enrollment.
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student) // The Enrollment has one Student
                .WithMany(u => u.Enrollments) // The User can have many Enrollments
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // <-- BLOCKS DELETION!

            // 3. Subject-Material Relationship (Material is a child of Subject)
            modelBuilder.Entity<CourseMaterial>()
                .HasOne(cm => cm.TeacherSubject)
                .WithMany(ts => ts.CourseMaterials)
                .OnDelete(DeleteBehavior.Cascade); // Safe to CASCADE here, if subject is deleted, materials must go.

            // 4. Subject-Enrollment Relationship (Enrollment is a child of Subject)
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.TeacherSubject)
                .WithMany(ts => ts.Enrollments)
                .OnDelete(DeleteBehavior.Cascade); // Safe to CASCADE here.

            // Configure UNIQUE indexes 
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<TeacherSubject>()
                .HasIndex(ts => ts.RegistrationCode)
                .IsUnique();
        }
    }
}