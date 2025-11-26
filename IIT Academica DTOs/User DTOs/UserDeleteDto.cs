using System.ComponentModel.DataAnnotations;

public class UserDeleteDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "User Id must be positive number")]

    public int Id { get; set; }
}