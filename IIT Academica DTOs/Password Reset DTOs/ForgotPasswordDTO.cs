using System.ComponentModel.DataAnnotations;

public class ForgetPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}