namespace IIT_Academica_API.Entities
{
    public class Enrollment
    {
        // Primary Key (PK) - Explicitly defined for simplicity
        public int Id { get; set; }

        // Foreign Key (FK) - Links to the Student
        public int StudentId { get; set; }

        // Foreign Key (FK) - Links to the Subject
        public int TeacherSubjectId { get; set; }

        // Core Property
        public DateTime EnrollmentDate { get; set; }

        // Navigation Properties

        // Many-to-1: The student who is enrolled
        public User? Student { get; set; }

        // Many-to-1: The subject the student is enrolled in
        public TeacherSubject? TeacherSubject { get; set; }
    }
}