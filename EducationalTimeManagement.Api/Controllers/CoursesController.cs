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
public class CoursesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CoursesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all courses
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetCourses()
    {
        var courses = await _context.Courses
            .Include(c => c.Teacher)
            .Select(c => new
            {
                c.CourseId,
                c.CourseCode,
                c.CourseName,
                c.Description,
                c.Credits,
                c.CreatedAt,
                c.IsActive,
                Teacher = new
                {
                    c.Teacher!.UserId,
                    c.Teacher.FirstName,
                    c.Teacher.LastName,
                    c.Teacher.Email
                },
                SessionsCount = c.ClassSessions.Count,
                EnrollmentsCount = c.Enrollments.Count
            })
            .ToListAsync();

        return Ok(courses);
    }

    /// <summary>
    /// Get course by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetCourse(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Teacher)
            .Include(c => c.ClassSessions)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
            .Where(c => c.CourseId == id)
            .Select(c => new
            {
                c.CourseId,
                c.CourseCode,
                c.CourseName,
                c.Description,
                c.Credits,
                c.CreatedAt,
                c.IsActive,
                Teacher = new
                {
                    c.Teacher!.UserId,
                    c.Teacher.FirstName,
                    c.Teacher.LastName,
                    c.Teacher.Email
                },
                Sessions = c.ClassSessions.Select(s => new
                {
                    s.Id,
                    s.SessionName,
                    s.SessionDate,
                    s.Description,
                    s.IsActive
                }),
                EnrolledStudents = c.Enrollments.Select(e => new
                {
                    e.EnrollmentId,
                    e.Student!.UserId,
                    e.Student.FirstName,
                    e.Student.LastName,
                    e.Student.Email,
                    e.EnrolledAt,
                    e.Status
                })
            })
            .FirstOrDefaultAsync();

        if (course == null)
        {
            return NotFound(new { message = "Course not found" });
        }

        return Ok(course);
    }

    /// <summary>
    /// Create a new course (Admin or Teacher)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<ActionResult<Course>> CreateCourse([FromBody] CreateCourseRequest request)
    {
        // Verify teacher exists
        var teacher = await _context.Users.FindAsync(request.TeacherId);
        if (teacher == null || teacher.Role != "Teacher")
        {
            return BadRequest(new { message = "Invalid teacher ID" });
        }

        // Check if course code already exists
        if (await _context.Courses.AnyAsync(c => c.CourseCode == request.CourseCode))
        {
            return BadRequest(new { message = "Course code already exists" });
        }

        var course = new Course
        {
            CourseCode = request.CourseCode,
            CourseName = request.CourseName,
            Description = request.Description,
            Credits = request.Credits,
            TeacherId = request.TeacherId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCourse), new { id = course.CourseId }, course);
    }

    /// <summary>
    /// Update a course (Admin or course teacher)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseRequest request)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound(new { message = "Course not found" });
        }

        // Check authorization - admin or course teacher
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        
        if (int.TryParse(userIdClaim, out int userId))
        {
            if (userRole != "Admin" && course.TeacherId != userId)
            {
                return Forbid();
            }
        }

        if (!string.IsNullOrEmpty(request.CourseCode))
        {
            if (await _context.Courses.AnyAsync(c => c.CourseCode == request.CourseCode && c.CourseId != id))
            {
                return BadRequest(new { message = "Course code already exists" });
            }
            course.CourseCode = request.CourseCode;
        }

        if (!string.IsNullOrEmpty(request.CourseName))
            course.CourseName = request.CourseName;
        if (!string.IsNullOrEmpty(request.Description))
            course.Description = request.Description;
        if (request.Credits.HasValue)
            course.Credits = request.Credits.Value;
        if (request.TeacherId.HasValue)
        {
            var teacher = await _context.Users.FindAsync(request.TeacherId.Value);
            if (teacher == null || teacher.Role != "Teacher")
            {
                return BadRequest(new { message = "Invalid teacher ID" });
            }
            course.TeacherId = request.TeacherId.Value;
        }
        if (request.IsActive.HasValue)
            course.IsActive = request.IsActive.Value;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Course updated successfully" });
    }

    /// <summary>
    /// Delete a course (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound(new { message = "Course not found" });
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Course deleted successfully" });
    }

    /// <summary>
    /// Get courses taught by current teacher
    /// </summary>
    [HttpGet("my-courses")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<IEnumerable<object>>> GetMyCourses()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var courses = await _context.Courses
            .Where(c => c.TeacherId == userId)
            .Select(c => new
            {
                c.CourseId,
                c.CourseCode,
                c.CourseName,
                c.Description,
                c.Credits,
                c.CreatedAt,
                c.IsActive,
                SessionsCount = c.ClassSessions.Count,
                EnrollmentsCount = c.Enrollments.Count
            })
            .ToListAsync();

        return Ok(courses);
    }
}
