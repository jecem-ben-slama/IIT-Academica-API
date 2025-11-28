// Entities/Notification.cs
using System;

namespace IIT_Academica_API.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; } // Path/URL to the uploaded image file
        public string? FileUrl { get; set; }  // Path/URL to the uploaded document/attachment file
        public DateTime PostedDate { get; set; }
        public int PostedByUserId { get; set; } // ID of the Admin who posted it
    }
}