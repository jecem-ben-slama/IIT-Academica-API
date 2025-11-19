// Models/Dtos/TeacherSubjectDto.cs

using System.ComponentModel.DataAnnotations;

public class TeacherSubjectDto
{
    public int Id { get; set; }

    [Required]
    public string RegistrationCode { get; set; }

    [Required]
    public string SubjectName { get; set; }

    // Include simplified Teacher information (e.g., for GetByIdWithTeacherAsync)
    public int TeacherId { get; set; }
    public string TeacherFullName { get; set; }
    // ... potentially other properties from the Teacher entity
}