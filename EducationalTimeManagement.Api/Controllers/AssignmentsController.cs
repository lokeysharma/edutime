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
public class AssignmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AssignmentsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all assignments
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAssignments()
    {
        var assignments = await _context.Assignments
            .Include(a => a.Course)
            .Select(a => new
            {
                a.AssignmentId,
                a.Title,
                a.Description,
                a.DueDate,
                a.MaxScore,
                a.CreatedAt,
                a.IsPublished,
                Course = new { a.Course!.CourseId, a.Course.CourseCode, a.Course.CourseName },
                SubmissionsCount = a.Submissions.Count
            })
            .ToListAsync();

        return Ok(assignments);
    }

    /// <summary>
    /// Get assignment by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetAssignment(int id)
    {
        var assignment = await _context.Assignments
            .Include(a => a.Course)
            .Include(a => a.Submissions)
                .ThenInclude(s => s.Student)
            .Where(a => a.AssignmentId == id)
            .Select(a => new
            {
                a.AssignmentId,
                a.Title,
                a.Description,
                a.DueDate,
                a.MaxScore,
                a.CreatedAt,
                a.IsPublished,
                Course = new { a.Course!.CourseId, a.Course.CourseCode, a.Course.CourseName },
                Submissions = a.Submissions.Select(s => new
                {
                    s.SubmissionId,
                    s.SubmittedAt,
                    s.Score,
                    s.GradedAt,
                    Student = new { s.Student!.UserId, s.Student.FirstName, s.Student.LastName }
                })
            })
            .FirstOrDefaultAsync();

        if (assignment == null)
        {
            return NotFound(new { message = "Assignment not found" });
        }

        return Ok(assignment);
    }

    /// <summary>
    /// Create a new assignment (Teacher or Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<Assignment>> CreateAssignment([FromBody] CreateAssignmentRequest request)
    {
        var course = await _context.Courses.FindAsync(request.CourseId);
        if (course == null)
        {
            return BadRequest(new { message = "Invalid course ID" });
        }

        // Verify current user is the course teacher or admin
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        
        if (int.TryParse(userIdClaim, out int userId))
        {
            if (userRole != "Admin" && course.TeacherId != userId)
            {
                return Forbid();
            }
        }

        var assignment = new Assignment
        {
            CourseId = request.CourseId,
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            MaxScore = request.MaxScore,
            CreatedAt = DateTime.UtcNow,
            IsPublished = true
        };

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAssignment), new { id = assignment.AssignmentId }, assignment);
    }

    /// <summary>
    /// Update an assignment
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> UpdateAssignment(int id, [FromBody] CreateAssignmentRequest request)
    {
        var assignment = await _context.Assignments.Include(a => a.Course).FirstOrDefaultAsync(a => a.AssignmentId == id);
        if (assignment == null)
        {
            return NotFound(new { message = "Assignment not found" });
        }

        assignment.Title = request.Title;
        assignment.Description = request.Description;
        assignment.DueDate = request.DueDate;
        assignment.MaxScore = request.MaxScore;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Assignment updated successfully" });
    }

    /// <summary>
    /// Delete an assignment
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> DeleteAssignment(int id)
    {
        var assignment = await _context.Assignments.FindAsync(id);
        if (assignment == null)
        {
            return NotFound(new { message = "Assignment not found" });
        }

        _context.Assignments.Remove(assignment);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Assignment deleted successfully" });
    }

    /// <summary>
    /// Get assignments for courses taught by current teacher
    /// </summary>
    [HttpGet("my-assignments")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<IEnumerable<object>>> GetMyAssignments()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var assignments = await _context.Assignments
            .Include(a => a.Course)
            .Where(a => a.Course!.TeacherId == userId)
            .Select(a => new
            {
                a.AssignmentId,
                a.Title,
                a.Description,
                a.DueDate,
                a.MaxScore,
                a.CreatedAt,
                a.IsPublished,
                Course = new { a.Course!.CourseCode, a.Course.CourseName },
                SubmissionsCount = a.Submissions.Count
            })
            .ToListAsync();

        return Ok(assignments);
    }
}
