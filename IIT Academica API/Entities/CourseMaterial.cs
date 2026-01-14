namespace IIT_Academica_API.Entities
{
    public class CourseMaterial
    {
        public int Id { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? FilePathOrUrl { get; set; }
        public string? MaterialType { get; set; }
        public DateTime UploadDate { get; set; }

        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }
    }
}
