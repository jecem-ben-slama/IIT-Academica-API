using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IIT_Academica_DTOs.Enrollment_DTOs
{
    public class StudentCourseDto
    {
        public int EnrollmentId { get; set; } // The ID of the join table record (useful for dropping the course)
        public string RegistrationCode { get; set; }
        public string CourseTitle { get; set; }
        public string TeacherFullName { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public int SubjectId {get;set;}
    }
}
