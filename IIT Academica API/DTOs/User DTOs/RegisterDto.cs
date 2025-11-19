using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string Role { get; set; }

    public string? Name { get; set; }
    public string? LastName { get; set; }
}