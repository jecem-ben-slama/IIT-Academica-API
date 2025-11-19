using System.ComponentModel.DataAnnotations;

public class UserUpdateDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public string Name { get; set; }
    public string? LastName { get; set; }

}