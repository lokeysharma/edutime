namespace EducationalTimeManagement.Api.Models.DTOs;

public class CreateCourseRequest
{
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Credits { get; set; }
    public int TeacherId { get; set; }
}

public class UpdateCourseRequest
{
    public string? CourseCode { get; set; }
    public string? CourseName { get; set; }
    public string? Description { get; set; }
    public int? Credits { get; set; }
    public int? TeacherId { get; set; }
    public bool? IsActive { get; set; }
}
