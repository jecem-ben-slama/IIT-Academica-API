namespace IIT_Academica_API.Entities
{
    public class User
    {
        // Primary Key (PK)
        public int Id { get; set; }

        // Core Properties
        public string Email { get; set; } // Unique identifier for login
        public string PasswordHash { get; set; } // Securely hashed password
        public string Role { get; set; } // "Admin", "Teacher", or "Student"

        // Navigation Properties (Relationships)

        // A Teacher can teach many subjects (1-to-Many)
        public ICollection<TeacherSubject>? TaughtSubjects { get; set; }

        // A Student can have many enrollments (1-to-Many)
        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}