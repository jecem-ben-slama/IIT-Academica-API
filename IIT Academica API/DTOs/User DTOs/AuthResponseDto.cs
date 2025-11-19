public class AuthResponseDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public string? Token { get; set; }
    public int? UserId { get; set; }
    public string? Role { get; set; }
    public string? Name { get; set; }
    public string? LastName { get; set; }

    public List<string>? Errors { get; set; }
}