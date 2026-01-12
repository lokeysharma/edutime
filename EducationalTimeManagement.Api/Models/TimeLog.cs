namespace EducationalTimeManagement.Api.Models;

public class TimeLog
{
    public int TimeLogId { get; set; }
    public int UserId { get; set; }
    public int? CourseId { get; set; }
    public string ActivityType { get; set; } = string.Empty; // Study, Assignment, Lecture, Break
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
    public Course? Course { get; set; }
}
