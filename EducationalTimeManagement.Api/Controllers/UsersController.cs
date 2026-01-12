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
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new
            {
                u.UserId,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role,
                u.Phone,
                u.CreatedAt,
                u.LastLoginAt,
                u.IsActive
            })
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetUser(int id)
    {
        var user = await _context.Users
            .Where(u => u.UserId == id)
            .Select(u => new
            {
                u.UserId,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role,
                u.Phone,
                u.CreatedAt,
                u.LastLoginAt,
                u.IsActive
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<object>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .Where(u => u.UserId == userId)
            .Select(u => new
            {
                u.UserId,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role,
                u.Phone,
                u.CreatedAt,
                u.LastLoginAt,
                u.IsActive,
                EnrollmentsCount = u.Enrollments.Count,
                TeachingCoursesCount = u.TeachingCourses.Count
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    /// <summary>
    /// Update user (Admin or self)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        
        if (!int.TryParse(userIdClaim, out int currentUserId))
        {
            return Unauthorized();
        }

        // Only allow admins or the user themselves to update
        if (currentUserId != id && userRole != "Admin")
        {
            return Forbid();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        if (!string.IsNullOrEmpty(request.FirstName))
            user.FirstName = request.FirstName;
        if (!string.IsNullOrEmpty(request.LastName))
            user.LastName = request.LastName;
        if (!string.IsNullOrEmpty(request.Phone))
            user.Phone = request.Phone;
        
        // Only admin can change role and active status
        if (userRole == "Admin")
        {
            if (!string.IsNullOrEmpty(request.Role))
                user.Role = request.Role;
            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "User updated successfully" });
    }

    /// <summary>
    /// Delete user (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User deleted successfully" });
    }

    /// <summary>
    /// Get all teachers
    /// </summary>
    [HttpGet("teachers")]
    public async Task<ActionResult<IEnumerable<object>>> GetTeachers()
    {
        var teachers = await _context.Users
            .Where(u => u.Role == "Teacher" && u.IsActive)
            .Select(u => new
            {
                u.UserId,
                u.Email,
                u.FirstName,
                u.LastName,
                CoursesCount = u.TeachingCourses.Count
            })
            .ToListAsync();

        return Ok(teachers);
    }

    /// <summary>
    /// Get all students
    /// </summary>
    [HttpGet("students")]
    public async Task<ActionResult<IEnumerable<object>>> GetStudents()
    {
        var students = await _context.Users
            .Where(u => u.Role == "Student" && u.IsActive)
            .Select(u => new
            {
                u.UserId,
                u.Email,
                u.FirstName,
                u.LastName,
                EnrollmentsCount = u.Enrollments.Count
            })
            .ToListAsync();

        return Ok(students);
    }
}

public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
}
