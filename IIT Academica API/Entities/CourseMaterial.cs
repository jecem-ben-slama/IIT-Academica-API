// Entities/CourseMaterial.cs

namespace IIT_Academica_API.Entities
{
    public class CourseMaterial
    {
        public int Id { get; set; }

        // Content details
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePathOrUrl { get; set; } // Path to the uploaded file or external URL
        public string MaterialType { get; set; } // e.g., "Document", "Video Link", "Quiz"
        public DateTime UploadDate { get; set; }

        // Foreign Key to Subject (Course Section)
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
    }
}