using System;

namespace IIT_Academica_API.Entities
{
    public class AttendanceRecord
    {
        public int Id { get; set; }

        // FIX: Foreign Key points to the class section instance
        public int TeacherSubjectId { get; set; }
        public TeacherSubject TeacherSubject { get; set; }

        // Links to the student being marked
        public int? StudentId { get; set; }
        public ApplicationUser? Student { get; set; }

        // When the attendance was taken
        public DateTime SessionDate { get; set; }

        // The status (e.g., "Present", "Absent", "Tardy")
        public string? Status { get; set; }

        // REMOVED: ProfessorId (It can be derived via TeacherSubject.TeacherId)
    }
}