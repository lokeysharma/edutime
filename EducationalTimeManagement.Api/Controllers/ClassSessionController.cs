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
public class ClassSessionController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClassSessionController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all class sessions
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAllSessions()
    {
        var sessions = await _context.ClassSessions
            .Include(cs => cs.Course)
            .Select(cs => new
            {
                cs.Id,
                cs.SessionName,
                cs.SessionDate,
                cs.Description,
                cs.IsActive,
                Course = cs.Course != null ? new { cs.Course.CourseId, cs.Course.CourseCode, cs.Course.CourseName } : null
            })
            .ToListAsync();

        return Ok(sessions);
    }

    /// <summary>
    /// Get session by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetSession(int id)
    {
        var session = await _context.ClassSessions
            .Include(cs => cs.Course)
            .Include(cs => cs.Schedules)
            .Where(cs => cs.Id == id)
            .Select(cs => new
            {
                cs.Id,
                cs.SessionName,
                cs.SessionDate,
                cs.Description,
                cs.IsActive,
                Course = cs.Course != null ? new { cs.Course.CourseId, cs.Course.CourseCode, cs.Course.CourseName } : null,
                Schedules = cs.Schedules.Select(s => new
                {
                    s.ScheduleId,
                    s.DayOfWeek,
                    s.StartTime,
                    s.EndTime,
                    s.Room
                })
            })
            .FirstOrDefaultAsync();

        if (session == null)
        {
            return NotFound(new { message = "Session not found" });
        }

        return Ok(session);
    }

    /// <summary>
    /// Create a new class session
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<ClassSession>> CreateSession([FromBody] CreateSessionRequest request)
    {
        // Verify course exists if provided
        if (request.CourseId.HasValue)
        {
            var course = await _context.Courses.FindAsync(request.CourseId.Value);
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
        }

        var session = new ClassSession
        {
            SessionName = request.SessionName,
            CourseId = request.CourseId,
            SessionDate = request.SessionDate,
            Description = request.Description,
            IsActive = true
        };

        _context.ClassSessions.Add(session);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
    }

    /// <summary>
    /// Update a class session
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> UpdateSession(int id, [FromBody] UpdateSessionRequest request)
    {
        var session = await _context.ClassSessions.Include(s => s.Course).FirstOrDefaultAsync(s => s.Id == id);
        if (session == null)
        {
            return NotFound(new { message = "Session not found" });
        }

        // Check authorization
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        
        if (int.TryParse(userIdClaim, out int userId))
        {
            if (userRole != "Admin" && session.Course?.TeacherId != userId)
            {
                return Forbid();
            }
        }

        if (!string.IsNullOrEmpty(request.SessionName))
            session.SessionName = request.SessionName;
        if (request.CourseId.HasValue)
            session.CourseId = request.CourseId;
        if (request.SessionDate.HasValue)
            session.SessionDate = request.SessionDate;
        if (!string.IsNullOrEmpty(request.Description))
            session.Description = request.Description;
        if (request.IsActive.HasValue)
            session.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Session updated successfully" });
    }

    /// <summary>
    /// Delete a class session
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSession(int id)
    {
        var session = await _context.ClassSessions.FindAsync(id);
        if (session == null)
        {
            return NotFound(new { message = "Session not found" });
        }

        _context.ClassSessions.Remove(session);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Session deleted successfully" });
    }

    /// <summary>
    /// Get sessions by course
    /// </summary>
    [HttpGet("course/{courseId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetSessionsByCourse(int courseId)
    {
        var sessions = await _context.ClassSessions
            .Where(cs => cs.CourseId == courseId)
            .Select(cs => new
            {
                cs.Id,
                cs.SessionName,
                cs.SessionDate,
                cs.Description,
                cs.IsActive
            })
            .ToListAsync();

        return Ok(sessions);
    }
}
