using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IIT_Academica_DTOs.Enrollment_DTOs
{
    public class EnrollmentRequestDto
    {
        [Required(ErrorMessage = "Subject ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Subject ID must be a valid positive number.")]
        public int SubjectId { get; set; } // 🚀 NEW FIELD

        [Required(ErrorMessage = "Registration Code is required.")]
        public string RegistrationCode { get; set; }
    }
}