namespace IIT_Academica_API.Entities
{

    public class Subject
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? RegistrationCode { get; set; }

        public int TeacherId { get; set; }


        public ApplicationUser? Teacher { get; set; }

        public ICollection<CourseMaterial>? CourseMaterials { get; set; }

        public ICollection<Enrollment>? Enrollments { get; set; }

        public ICollection<AttendanceRecord>? AttendanceSessions { get; set; }
    }
}
