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
public class AttendanceController : ControllerBase
{
    private readonly AppDbContext _context;

    public AttendanceController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get attendance for a session
    /// </summary>
    [HttpGet("session/{sessionId}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<IEnumerable<object>>> GetAttendanceBySession(int sessionId)
    {
        var attendance = await _context.Attendances
            .Where(a => a.ClassSessionId == sessionId)
            .Include(a => a.Student)
            .Select(a => new
            {
                a.AttendanceId,
                a.Date,
                a.Status,
                a.Notes,
                a.RecordedAt,
                Student = new { a.Student!.UserId, a.Student.FirstName, a.Student.LastName, a.Student.Email }
            })
            .ToListAsync();

        return Ok(attendance);
    }

    /// <summary>
    /// Record attendance
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<object>> RecordAttendance([FromBody] CreateAttendanceRequest request)
    {
        // Verify student exists
        var student = await _context.Users.FindAsync(request.StudentId);
        if (student == null || student.Role != "Student")
        {
            return BadRequest(new { message = "Invalid student ID" });
        }

        // Verify session exists
        var session = await _context.ClassSessions.FindAsync(request.ClassSessionId);
        if (session == null)
        {
            return BadRequest(new { message = "Invalid session ID" });
        }

        // Check if attendance already recorded
        var existing = await _context.Attendances
            .FirstOrDefaultAsync(a => a.StudentId == request.StudentId && 
                                      a.ClassSessionId == request.ClassSessionId && 
                                      a.Date.Date == request.Date.Date);
        
        if (existing != null)
        {
            // Update existing
            existing.Status = request.Status;
            existing.Notes = request.Notes;
            existing.RecordedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Attendance updated", attendanceId = existing.AttendanceId });
        }

        var attendance = new Attendance
        {
            StudentId = request.StudentId,
            ClassSessionId = request.ClassSessionId,
            Date = request.Date,
            Status = request.Status,
            Notes = request.Notes,
            RecordedAt = DateTime.UtcNow
        };

        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAttendanceBySession), new { sessionId = request.ClassSessionId }, new
        {
            attendance.AttendanceId,
            attendance.StudentId,
            attendance.ClassSessionId,
            attendance.Date,
            attendance.Status
        });
    }

    /// <summary>
    /// Bulk record attendance for a session
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<object>> BulkRecordAttendance([FromBody] List<CreateAttendanceRequest> requests)
    {
        var results = new List<object>();

        foreach (var request in requests)
        {
            var existing = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == request.StudentId && 
                                          a.ClassSessionId == request.ClassSessionId && 
                                          a.Date.Date == request.Date.Date);
            
            if (existing != null)
            {
                existing.Status = request.Status;
                existing.Notes = request.Notes;
                existing.RecordedAt = DateTime.UtcNow;
                results.Add(new { studentId = request.StudentId, action = "updated" });
            }
            else
            {
                var attendance = new Attendance
                {
                    StudentId = request.StudentId,
                    ClassSessionId = request.ClassSessionId,
                    Date = request.Date,
                    Status = request.Status,
                    Notes = request.Notes,
                    RecordedAt = DateTime.UtcNow
                };
                _context.Attendances.Add(attendance);
                results.Add(new { studentId = request.StudentId, action = "created" });
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = $"Recorded {results.Count} attendance records", details = results });
    }

    /// <summary>
    /// Get my attendance (Student)
    /// </summary>
    [HttpGet("my-attendance")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<IEnumerable<object>>> GetMyAttendance()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int studentId))
        {
            return Unauthorized();
        }

        var attendance = await _context.Attendances
            .Where(a => a.StudentId == studentId)
            .Include(a => a.ClassSession)
                .ThenInclude(s => s!.Course)
            .Select(a => new
            {
                a.AttendanceId,
                a.Date,
                a.Status,
                a.Notes,
                Session = new
                {
                    a.ClassSession!.Id,
                    a.ClassSession.SessionName,
                    Course = a.ClassSession.Course != null ? new { a.ClassSession.Course.CourseCode, a.ClassSession.Course.CourseName } : null
                }
            })
            .ToListAsync();

        return Ok(attendance);
    }
}
