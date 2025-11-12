namespace IIT_Academica_API.Entities
{
    public class CourseMaterial
    {
        // Primary Key (PK)
        public int Id { get; set; }

        // Core Properties
        public string Title { get; set; }
        public string FilePath { get; set; } // Path/URL to the file
        public DateTime AccessUntilDate { get; set; } // ISTQB BVA Test Focus

        // Foreign Key (FK) - Link to the subject
        public int TeacherSubjectId { get; set; }

        // Navigation Property
        public TeacherSubject? TeacherSubject { get; set; }
    }
}