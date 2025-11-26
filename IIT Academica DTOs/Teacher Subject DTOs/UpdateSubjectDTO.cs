// Models/Dtos/UpdateTeacherSubjectDto.cs
using System.ComponentModel.DataAnnotations;

public class UpdateSubjectDTO
{
    [Required(ErrorMessage ="Subject Id Is Required")]
    [Range(1, int.MaxValue,ErrorMessage ="Subject Id must be positive number")]
    public int Id { get; set; }

    [StringLength(100,MinimumLength =10, ErrorMessage = "Subject Name cannot exceed 100 characters.")]
    public string SubjectName { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Teacher ID must be a valid positive number.")]
    public int TeacherId { get; set; }
}