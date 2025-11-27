// Models/DTOs/CourseMaterialDto.cs
using System;

public class CourseMaterialDto
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string FilePathOrUrl { get; set; }
    public string MaterialType { get; set; }
    public DateTime UploadDate { get; set; }
}