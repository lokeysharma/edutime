namespace EducationalTimeManagement.Api.Models.DTOs;

public class CreateEnrollmentRequest
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
}

public class CreateAttendanceRequest
{
    public int StudentId { get; set; }
    public int ClassSessionId { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = "Present"; // Present, Absent, Late, Excused
    public string? Notes { get; set; }
}

public class CreateAssignmentRequest
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int MaxScore { get; set; } = 100;
}

public class CreateSubmissionRequest
{
    public int AssignmentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
}

public class GradeSubmissionRequest
{
    public int Score { get; set; }
    public string? Feedback { get; set; }
}
