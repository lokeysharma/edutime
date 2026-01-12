namespace EducationalTimeManagement.Api.Models;

public class Schedule
{
    public int ScheduleId { get; set; }
    public int ClassSessionId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Room { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    // Navigation property
    public ClassSession? ClassSession { get; set; }
}
