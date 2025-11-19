using IIT_Academica_API.Entities;
using System.Collections.Generic;

namespace IIT_Academica_API.Entities
{
    // Reworked: This is the core entity representing a unique class section 
    // (e.g., "Intro to C++ - Fall 2025")
    public class TeacherSubject
    {
        public int Id { get; set; }

        // REQUIRED: The name/title of this class section
        public string Title { get; set; }

        // REQUIRED: The unique code students use to register (e.g., "CS101-FALL-A")
        public string RegistrationCode { get; set; }

        // REQUIRED: Foreign Key to the assigned teacher (Assuming string for ApplicationUser ID)
        public int TeacherId { get; set; }


        // Navigation Properties
        public ApplicationUser? Teacher { get; set; } // The assigned professor

        // Links to materials uploaded by the teacher
        public ICollection<CourseMaterial>? CourseMaterials { get; set; }

        // Links to students registered for this specific section
        public ICollection<Enrollment>? Enrollments { get; set; }

        // Links to attendance records taken in this class
        public ICollection<AttendanceRecord>? AttendanceSessions { get; set; }
    }
}