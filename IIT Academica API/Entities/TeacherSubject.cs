namespace IIT_Academica_API.Entities
{
    public class TeacherSubject
    {
        // Primary Key (PK)
        public int Id { get; set; }

        // Core Properties
        public string Title { get; set; }
        public string RegistrationCode { get; set; } // The secret code for enrollment (must be unique)

        // Foreign Key (FK) - Link to the User (Teacher)
        public int TeacherId { get; set; }

        // Navigation Properties

        // Many-to-1: The specific teacher
        public User? Teacher { get; set; }

        // 1-to-Many: Materials in this subject
        public ICollection<CourseMaterial>? CourseMaterials { get; set; }

        // 1-to-Many: Students enrolled in this subject
        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}