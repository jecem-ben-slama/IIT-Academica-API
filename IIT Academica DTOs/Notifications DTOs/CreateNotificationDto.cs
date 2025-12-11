using System.ComponentModel.DataAnnotations;

public class CreateNotificationDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(150)]
    public string? Title { get; set; }

    [Required(ErrorMessage = "Content is required.")]
    public string? Content { get; set; }

}