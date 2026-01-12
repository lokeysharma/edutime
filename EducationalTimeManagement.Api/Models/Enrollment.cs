namespace EducationalTimeManagement.Api.Models;

public class Enrollment
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Active"; // Active, Completed, Dropped

    // Navigation properties
    public User? Student { get; set; }
    public Course? Course { get; set; }
}
