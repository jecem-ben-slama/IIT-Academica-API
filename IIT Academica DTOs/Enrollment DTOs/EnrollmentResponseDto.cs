namespace IIT_Academica_API.Models.DTOs
{
    public class EnrollmentResponseDto
    {
        // The unique ID of the newly created Enrollment record
        public int EnrollmentId { get; set; }

        // The ID of the Student who enrolled (retrieved from the JWT token)
        public int StudentId { get; set; }

        // The ID of the specific Course Section that was enrolled in
        public int SubjectId { get; set; }

        // The title of the subject/course section for confirmation
        public string SubjectTitle { get; set; }

        // The date the enrollment was completed
        public DateTime EnrollmentDate { get; set; }
    }
}