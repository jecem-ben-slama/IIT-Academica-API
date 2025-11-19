public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string? Content { get; set; }
    public string? FileUrl { get; set; }
    public string Type { get; set; }
    public string TargetRole { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? AdminId { get; set; }
    public ApplicationUser? Admin { get; set; }
}