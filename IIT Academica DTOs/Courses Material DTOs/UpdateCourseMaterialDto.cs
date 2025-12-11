public class UpdateCourseMaterialDto
{
    public int Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int SubjectId { get; set; }
}