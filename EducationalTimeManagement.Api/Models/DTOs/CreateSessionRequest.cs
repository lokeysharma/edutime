namespace EducationalTimeManagement.Api.Models.DTOs;

public class CreateSessionRequest
{
    public string SessionName { get; set; } = string.Empty;
    public int? CourseId { get; set; }
    public DateTime? SessionDate { get; set; }
    public string? Description { get; set; }
}

public class UpdateSessionRequest
{
    public string? SessionName { get; set; }
    public int? CourseId { get; set; }
    public DateTime? SessionDate { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
