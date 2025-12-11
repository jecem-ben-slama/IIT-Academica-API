using System.ComponentModel.DataAnnotations;

public class CreateCourseMaterialDto
{
    [Required]
    public int SubjectId { get; set; }

    [Required]
    [StringLength(100)]
    public string? Title { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

  
}