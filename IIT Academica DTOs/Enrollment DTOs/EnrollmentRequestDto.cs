using System.ComponentModel.DataAnnotations;

namespace IIT_Academica_DTOs.Enrollment_DTOs
{
    public class EnrollmentRequestDto
    {
        [Required(ErrorMessage = "Subject ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Subject ID must be a valid positive number.")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Registration Code is required.")]
        public string? RegistrationCode { get; set; }
    }
}