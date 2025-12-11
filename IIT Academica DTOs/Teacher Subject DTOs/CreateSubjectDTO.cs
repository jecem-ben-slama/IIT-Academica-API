using System.ComponentModel.DataAnnotations;

public class CreateSubjectDto
{
    [Required(ErrorMessage = "Registration Code is required.")]
    [StringLength(50, ErrorMessage = "Registration Code cannot exceed 50 characters.")]
    public string? RegistrationCode { get; set; }

    [Required(ErrorMessage = "Subject Name is required.")]
    [StringLength(100, ErrorMessage = "Subject Name cannot exceed 100 characters.")]
    public string? SubjectName { get; set; }

    [Required(ErrorMessage = "Teacher ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Teacher ID must be a valid positive number.")]
    public int TeacherId { get; set; }
}