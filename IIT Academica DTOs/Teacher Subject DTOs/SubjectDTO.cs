using System.ComponentModel.DataAnnotations;

public class SubjectDTO
{
    public int Id { get; set; }

    [Required]
    public string? RegistrationCode { get; set; }

    [Required]
    public string? SubjectName { get; set; }

    public int TeacherId { get; set; }
    public string? TeacherFullName { get; set; }
    public int EnrollmentCount { get; set; } 
}