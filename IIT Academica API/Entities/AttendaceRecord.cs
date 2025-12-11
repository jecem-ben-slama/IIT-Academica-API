using System;

namespace IIT_Academica_API.Entities
{
    public class AttendanceRecord
    {
        public int Id { get; set; }

        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        public int? StudentId { get; set; }
        public ApplicationUser? Student { get; set; }

        public DateTime SessionDate { get; set; }

        public string? Status { get; set; }

    }
}