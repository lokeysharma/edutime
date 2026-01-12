namespace EducationalTimeManagement.Api.Models;

public class Attendance
{
    public int AttendanceId { get; set; }
    public int StudentId { get; set; }
    public int ClassSessionId { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = "Present"; // Present, Absent, Late, Excused
    public string? Notes { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? Student { get; set; }
    public ClassSession? ClassSession { get; set; }
}
