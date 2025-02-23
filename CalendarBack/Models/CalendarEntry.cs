using System.ComponentModel.DataAnnotations;

namespace CalendarBack.Models;

public class CalendarEntry
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ReminderDateTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool NotificationSent { get; set; }
}
