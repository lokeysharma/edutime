namespace EducationalTimeManagement.Api.Models;

public class Submission
{
    public int SubmissionId { get; set; }
    public int AssignmentId { get; set; }
    public int StudentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public int? Score { get; set; }
    public string? Feedback { get; set; }
    public DateTime? GradedAt { get; set; }

    // Navigation properties
    public Assignment? Assignment { get; set; }
    public User? Student { get; set; }
}
