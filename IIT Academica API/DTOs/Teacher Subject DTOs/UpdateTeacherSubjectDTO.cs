// Models/Dtos/UpdateTeacherSubjectDto.cs
using System.ComponentModel.DataAnnotations;

public class UpdateTeacherSubjectDto
{
    // ID is required to identify the entity being updated
    [Required]
    public int Id { get; set; }

    // SubjectName is typically updatable
    [Required(ErrorMessage = "Subject Name is required.")]
    [StringLength(100, ErrorMessage = "Subject Name cannot exceed 100 characters.")]
    public string SubjectName { get; set; }

    // Teacher assignment can be changed
    [Required(ErrorMessage = "Teacher ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Teacher ID must be a valid positive number.")]
    public int TeacherId { get; set; }
}