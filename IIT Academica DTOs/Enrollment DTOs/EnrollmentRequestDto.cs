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
        // Must be required for the enrollment to proceed
        [Required(ErrorMessage = "The registration code is required for enrollment.")]
        // Add length validation if applicable, e.g., [StringLength(10)]
        public string RegistrationCode { get; set; }
    }
}