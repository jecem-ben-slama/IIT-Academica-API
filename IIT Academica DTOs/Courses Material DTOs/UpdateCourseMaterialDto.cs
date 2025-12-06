// UpdateCourseMaterialDto.cs (API Project)
public class UpdateCourseMaterialDto
{
    // The ID is required to match the route parameter and identify the record
    public int Id { get; set; } 
    
    [System.ComponentModel.DataAnnotations.Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // SubjectId is not needed for update but can be kept for consistency
     public int SubjectId { get; set; }
}