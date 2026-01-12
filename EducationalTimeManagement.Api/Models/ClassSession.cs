namespace EducationalTimeManagement.Api.Models;

public class ClassSession
{
    public int Id { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public int? CourseId { get; set; }
    public DateTime? SessionDate { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Course? Course { get; set; }
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
