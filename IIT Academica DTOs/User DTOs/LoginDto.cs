namespace IIT_Academica_DTOs;
using System.ComponentModel.DataAnnotations;

public class LoginDto
{
    [Required]
    [StringLength(100,ErrorMessage ="Email Too Long")]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}