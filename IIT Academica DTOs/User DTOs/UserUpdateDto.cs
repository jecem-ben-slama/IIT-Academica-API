using System.ComponentModel.DataAnnotations;

public class UserUpdateDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Subject Id must be positive number")]

    public int Id { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }
    public string? role { get; set; }

}