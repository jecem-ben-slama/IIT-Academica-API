using System;

namespace IIT_Academica_API.Entities
{
    public class CourseMaterial
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FilePath { get; set; }
        public DateTime AccessUntilDate { get; set; }

        // Reworked: Foreign Key points to the TeacherSubject
        public int SubjectId { get; set; }

        public Subject Subject { get; set; }
    }
}