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
     [Required(ErrorMessage = "The registration code is required for enrollment.")]
     [StringLength(10, MinimumLength = 6)]

        public string RegistrationCode { get; set; }
    }
}