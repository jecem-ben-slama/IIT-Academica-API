
namespace IIT_Academica_DTOs.Enrollment_DTOs
{
    public class StudentCourseDto
    {
        public int EnrollmentId { get; set; }
        public string? RegistrationCode { get; set; }
        public string? CourseTitle { get; set; }
        public string? TeacherFullName { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public int SubjectId { get; set; }
    }
}
