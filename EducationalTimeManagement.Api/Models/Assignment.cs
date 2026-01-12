namespace EducationalTimeManagement.Api.Models;

public class Assignment
{
    public int AssignmentId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int MaxScore { get; set; } = 100;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsPublished { get; set; } = false;

    // Navigation properties
    public Course? Course { get; set; }
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
