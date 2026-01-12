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
public class EnrollmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EnrollmentsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all enrollments (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<object>>> GetEnrollments()
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Select(e => new
            {
                e.EnrollmentId,
                e.EnrolledAt,
                e.Status,
                Student = new { e.Student!.UserId, e.Student.FirstName, e.Student.LastName, e.Student.Email },
                Course = new { e.Course!.CourseId, e.Course.CourseCode, e.Course.CourseName }
            })
            .ToListAsync();

        return Ok(enrollments);
    }

    /// <summary>
    /// Enroll a student in a course
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<object>> CreateEnrollment([FromBody] CreateEnrollmentRequest request)
    {
        // Verify student exists
        var student = await _context.Users.FindAsync(request.StudentId);
        if (student == null || student.Role != "Student")
        {
            return BadRequest(new { message = "Invalid student ID" });
        }

        // Verify course exists
        var course = await _context.Courses.FindAsync(request.CourseId);
        if (course == null)
        {
            return BadRequest(new { message = "Invalid course ID" });
        }

        // Check if already enrolled
        if (await _context.Enrollments.AnyAsync(e => e.StudentId == request.StudentId && e.CourseId == request.CourseId))
        {
            return BadRequest(new { message = "Student is already enrolled in this course" });
        }

        var enrollment = new Enrollment
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            EnrolledAt = DateTime.UtcNow,
            Status = "Active"
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEnrollments), new { id = enrollment.EnrollmentId }, new
        {
            enrollment.EnrollmentId,
            enrollment.StudentId,
            enrollment.CourseId,
            enrollment.EnrolledAt,
            enrollment.Status
        });
    }

    /// <summary>
    /// Get enrollments for current student
    /// </summary>
    [HttpGet("my-enrollments")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<IEnumerable<object>>> GetMyEnrollments()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var enrollments = await _context.Enrollments
            .Where(e => e.StudentId == userId)
            .Include(e => e.Course)
                .ThenInclude(c => c!.Teacher)
            .Select(e => new
            {
                e.EnrollmentId,
                e.EnrolledAt,
                e.Status,
                Course = new
                {
                    e.Course!.CourseId,
                    e.Course.CourseCode,
                    e.Course.CourseName,
                    e.Course.Description,
                    e.Course.Credits,
                    Teacher = new { e.Course.Teacher!.FirstName, e.Course.Teacher.LastName }
                }
            })
            .ToListAsync();

        return Ok(enrollments);
    }

    /// <summary>
    /// Update enrollment status
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> UpdateEnrollment(int id, [FromBody] string status)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);
        if (enrollment == null)
        {
            return NotFound(new { message = "Enrollment not found" });
        }

        enrollment.Status = status;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Enrollment updated successfully" });
    }

    /// <summary>
    /// Delete enrollment
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEnrollment(int id)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);
        if (enrollment == null)
        {
            return NotFound(new { message = "Enrollment not found" });
        }

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Enrollment deleted successfully" });
    }
}
