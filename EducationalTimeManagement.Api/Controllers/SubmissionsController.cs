using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EducationalTimeManagement.Api.Data;
using EducationalTimeManagement.Api.Models;
using EducationalTimeManagement.Api.Models.DTOs;

namespace EducationalTimeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SubmissionsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all submissions for an assignment (Teacher or Admin)
    /// </summary>
    [HttpGet("assignment/{assignmentId}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<IEnumerable<object>>> GetSubmissionsByAssignment(int assignmentId)
    {
        var submissions = await _context.Submissions
            .Where(s => s.AssignmentId == assignmentId)
            .Include(s => s.Student)
            .Select(s => new
            {
                s.SubmissionId,
                s.Content,
                s.FileUrl,
                s.SubmittedAt,
                s.Score,
                s.Feedback,
                s.GradedAt,
                Student = new { s.Student!.UserId, s.Student.FirstName, s.Student.LastName, s.Student.Email }
            })
            .ToListAsync();

        return Ok(submissions);
    }

    /// <summary>
    /// Submit an assignment (Student)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<object>> CreateSubmission([FromBody] CreateSubmissionRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int studentId))
        {
            return Unauthorized();
        }

        // Verify assignment exists
        var assignment = await _context.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.AssignmentId == request.AssignmentId);
        
        if (assignment == null)
        {
            return BadRequest(new { message = "Invalid assignment ID" });
        }

        // Verify student is enrolled in the course
        var isEnrolled = await _context.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.CourseId == assignment.CourseId && e.Status == "Active");
        
        if (!isEnrolled)
        {
            return BadRequest(new { message = "You are not enrolled in this course" });
        }

        // Check if already submitted
        var existingSubmission = await _context.Submissions
            .FirstOrDefaultAsync(s => s.AssignmentId == request.AssignmentId && s.StudentId == studentId);
        
        if (existingSubmission != null)
        {
            // Update existing submission
            existingSubmission.Content = request.Content;
            existingSubmission.FileUrl = request.FileUrl;
            existingSubmission.SubmittedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Submission updated", submissionId = existingSubmission.SubmissionId });
        }

        var submission = new Submission
        {
            AssignmentId = request.AssignmentId,
            StudentId = studentId,
            Content = request.Content,
            FileUrl = request.FileUrl,
            SubmittedAt = DateTime.UtcNow
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSubmissionsByAssignment), new { assignmentId = request.AssignmentId }, new
        {
            submission.SubmissionId,
            submission.AssignmentId,
            submission.Content,
            submission.SubmittedAt
        });
    }

    /// <summary>
    /// Grade a submission (Teacher)
    /// </summary>
    [HttpPut("{id}/grade")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GradeSubmission(int id, [FromBody] GradeSubmissionRequest request)
    {
        var submission = await _context.Submissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.SubmissionId == id);
        
        if (submission == null)
        {
            return NotFound(new { message = "Submission not found" });
        }

        if (request.Score > submission.Assignment!.MaxScore)
        {
            return BadRequest(new { message = $"Score cannot exceed max score of {submission.Assignment.MaxScore}" });
        }

        submission.Score = request.Score;
        submission.Feedback = request.Feedback;
        submission.GradedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Submission graded successfully" });
    }

    /// <summary>
    /// Get my submissions (Student)
    /// </summary>
    [HttpGet("my-submissions")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<IEnumerable<object>>> GetMySubmissions()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int studentId))
        {
            return Unauthorized();
        }

        var submissions = await _context.Submissions
            .Where(s => s.StudentId == studentId)
            .Include(s => s.Assignment)
                .ThenInclude(a => a!.Course)
            .Select(s => new
            {
                s.SubmissionId,
                s.Content,
                s.FileUrl,
                s.SubmittedAt,
                s.Score,
                s.Feedback,
                s.GradedAt,
                Assignment = new
                {
                    s.Assignment!.AssignmentId,
                    s.Assignment.Title,
                    s.Assignment.DueDate,
                    s.Assignment.MaxScore,
                    Course = new { s.Assignment.Course!.CourseCode, s.Assignment.Course.CourseName }
                }
            })
            .ToListAsync();

        return Ok(submissions);
    }
}
