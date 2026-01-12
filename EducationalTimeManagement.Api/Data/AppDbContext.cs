using Microsoft.EntityFrameworkCore;
using EducationalTimeManagement.Api.Models;

namespace EducationalTimeManagement.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Core Tables
    public DbSet<User> Users { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<ClassSession> ClassSessions { get; set; }
    
    // Enrollment & Scheduling
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    
    // Assignments & Submissions
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    
    // Time Management
    public DbSet<TimeLog> TimeLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Course configuration
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId);
            entity.Property(e => e.CourseCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CourseName).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.CourseCode).IsUnique();
            
            entity.HasOne(e => e.Teacher)
                .WithMany(u => u.TeachingCourses)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ClassSession configuration
        modelBuilder.Entity<ClassSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionName).IsRequired().HasMaxLength(200);
            
            entity.HasOne(e => e.Course)
                .WithMany(c => c.ClassSessions)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Enrollment configuration
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId);
            entity.Property(e => e.Status).HasMaxLength(50);
            
            entity.HasOne(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
        });

        // Schedule configuration
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId);
            entity.Property(e => e.Room).HasMaxLength(50);
            
            entity.HasOne(e => e.ClassSession)
                .WithMany(cs => cs.Schedules)
                .HasForeignKey(e => e.ClassSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Attendance configuration
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId);
            entity.Property(e => e.Status).HasMaxLength(50);
            
            entity.HasOne(e => e.Student)
                .WithMany(u => u.Attendances)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ClassSession)
                .WithMany(cs => cs.Attendances)
                .HasForeignKey(e => e.ClassSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.StudentId, e.ClassSessionId, e.Date }).IsUnique();
        });

        // Assignment configuration
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            
            entity.HasOne(e => e.Course)
                .WithMany(c => c.Assignments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Submission configuration
        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.SubmissionId);
            
            entity.HasOne(e => e.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(e => e.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Student)
                .WithMany(u => u.Submissions)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => new { e.AssignmentId, e.StudentId }).IsUnique();
        });

        // TimeLog configuration
        modelBuilder.Entity<TimeLog>(entity =>
        {
            entity.HasKey(e => e.TimeLogId);
            entity.Property(e => e.ActivityType).HasMaxLength(50);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.TimeLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).HasMaxLength(50);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
