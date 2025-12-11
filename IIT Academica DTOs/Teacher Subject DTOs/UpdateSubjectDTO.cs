using System.ComponentModel.DataAnnotations;

public class UpdateSubjectDTO
{
    [Required(ErrorMessage = "Subject Id Is Required")]
    [Range(1, int.MaxValue, ErrorMessage = "Subject Id must be positive number")]
    public int Id { get; set; }
    [Required(ErrorMessage = "Registration Code is required.")]
    public string? RegistrationCode { get; set; }
    public string? SubjectName { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Teacher ID must be a valid positive number.")]
    public int TeacherId { get; set; }
}