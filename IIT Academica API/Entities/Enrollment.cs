using System;

namespace IIT_Academica_API.Entities
{
    public class Enrollment
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public ApplicationUser Student { get; set; }

        // Reworked: Foreign Key points to the TeacherSubject
        public int TeacherSubjectId { get; set; }
        public TeacherSubject TeacherSubject { get; set; }

        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; }
    }
}