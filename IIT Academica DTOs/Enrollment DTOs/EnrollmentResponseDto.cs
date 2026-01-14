namespace IIT_Academica_API.Models.DTOs
{
    public class EnrollmentResponseDto
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public string? SubjectTitle { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string? Status { get; set; }
    }
}