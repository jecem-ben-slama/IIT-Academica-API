public class NotificationDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public string? FileUrl { get; set; }
    public DateTime PostedDate { get; set; }
    public int PostedByUserId { get; set; }
}