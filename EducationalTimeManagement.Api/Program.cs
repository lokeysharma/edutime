using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using EducationalTimeManagement.Api.Data;
using EducationalTimeManagement.Api.Models;
using EducationalTimeManagement.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with OAuth2 Password Flow
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "EducationalTimeManagement API", 
        Version = "v1",
        Description = @"## Authentication Instructions
Click **Authorize** button → Enter your email & password → Click **Authorize**

### Test Credentials:
- student@example.com / Student123!
- teacher@example.com / Teacher123!
- admin@example.com / Admin123!"
    });
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    
    // OAuth2 Password Flow - allows entering username/password directly
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Password = new OpenApiOAuthFlow
            {
                TokenUrl = new Uri("/api/Auth/token", UriKind.Relative),
                Scopes = new Dictionary<string, string>()
            }
        },
        Description = "Enter your email as username and password"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Database - supports both SQLite and SQL Server
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection") ?? connectionString);
    }
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

// Register JwtService
builder.Services.AddScoped<JwtService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Kestrel to use port 5000
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000);
});

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Use Migrate() for SQL Server with proper migrations, EnsureCreated() for SQLite quick setup
    if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        context.Database.Migrate();
    }
    else
    {
        context.Database.EnsureCreated();
    }

    // Seed Users first (they're referenced by other tables)
    if (!context.Users.Any())
    {
        var users = new List<User>
        {
            new User
            {
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "Admin",
                FirstName = "Admin",
                LastName = "User"
            },
            new User
            {
                Email = "teacher@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher123!"),
                Role = "Teacher",
                FirstName = "John",
                LastName = "Smith"
            },
            new User
            {
                Email = "teacher2@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Teacher123!"),
                Role = "Teacher",
                FirstName = "Sarah",
                LastName = "Johnson"
            },
            new User
            {
                Email = "student@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student123!"),
                Role = "Student",
                FirstName = "Alice",
                LastName = "Brown"
            },
            new User
            {
                Email = "student2@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student123!"),
                Role = "Student",
                FirstName = "Bob",
                LastName = "Wilson"
            },
            new User
            {
                Email = "student3@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student123!"),
                Role = "Student",
                FirstName = "Charlie",
                LastName = "Davis"
            }
        };
        context.Users.AddRange(users);
        context.SaveChanges();
    }

    // Seed Courses
    if (!context.Courses.Any())
    {
        var teacher1 = context.Users.First(u => u.Email == "teacher@example.com");
        var teacher2 = context.Users.First(u => u.Email == "teacher2@example.com");
        
        var courses = new List<Course>
        {
            new Course { CourseCode = "MATH101", CourseName = "Introduction to Algebra", Description = "Basic algebraic concepts", Credits = 3, TeacherId = teacher1.UserId },
            new Course { CourseCode = "PHYS101", CourseName = "Physics Fundamentals", Description = "Introduction to physics and motion", Credits = 4, TeacherId = teacher1.UserId },
            new Course { CourseCode = "CHEM101", CourseName = "General Chemistry", Description = "Basic chemistry principles", Credits = 4, TeacherId = teacher2.UserId },
            new Course { CourseCode = "CS101", CourseName = "Programming Basics", Description = "Introduction to programming concepts", Credits = 3, TeacherId = teacher2.UserId },
            new Course { CourseCode = "ENG101", CourseName = "English Composition", Description = "Writing and grammar fundamentals", Credits = 3, TeacherId = teacher1.UserId }
        };
        context.Courses.AddRange(courses);
        context.SaveChanges();
    }

    // Seed ClassSessions
    if (!context.ClassSessions.Any())
    {
        var courses = context.Courses.ToList();
        var sessions = new List<ClassSession>
        {
            new ClassSession { SessionName = "Math - Algebra Basics", CourseId = courses[0].CourseId, Description = "Introduction to variables" },
            new ClassSession { SessionName = "Math - Linear Equations", CourseId = courses[0].CourseId, Description = "Solving linear equations" },
            new ClassSession { SessionName = "Physics - Motion 01", CourseId = courses[1].CourseId, Description = "Newton's laws of motion" },
            new ClassSession { SessionName = "Physics - Energy", CourseId = courses[1].CourseId, Description = "Kinetic and potential energy" },
            new ClassSession { SessionName = "Chemistry - Periodic Table", CourseId = courses[2].CourseId, Description = "Elements and their properties" },
            new ClassSession { SessionName = "Chemistry - Chemical Bonds", CourseId = courses[2].CourseId, Description = "Types of chemical bonding" },
            new ClassSession { SessionName = "CS - Variables & Types", CourseId = courses[3].CourseId, Description = "Data types and variables" },
            new ClassSession { SessionName = "CS - Control Flow", CourseId = courses[3].CourseId, Description = "If statements and loops" },
            new ClassSession { SessionName = "English - Grammar 101", CourseId = courses[4].CourseId, Description = "Parts of speech" },
            new ClassSession { SessionName = "English - Essay Writing", CourseId = courses[4].CourseId, Description = "Structuring an essay" }
        };
        context.ClassSessions.AddRange(sessions);
        context.SaveChanges();
    }

    // Seed Enrollments
    if (!context.Enrollments.Any())
    {
        var students = context.Users.Where(u => u.Role == "Student").ToList();
        var courses = context.Courses.ToList();
        
        var enrollments = new List<Enrollment>();
        foreach (var student in students)
        {
            // Enroll each student in 3-4 courses
            enrollments.Add(new Enrollment { StudentId = student.UserId, CourseId = courses[0].CourseId });
            enrollments.Add(new Enrollment { StudentId = student.UserId, CourseId = courses[1].CourseId });
            enrollments.Add(new Enrollment { StudentId = student.UserId, CourseId = courses[3].CourseId });
        }
        context.Enrollments.AddRange(enrollments);
        context.SaveChanges();
    }

    // Seed Assignments
    if (!context.Assignments.Any())
    {
        var courses = context.Courses.ToList();
        var assignments = new List<Assignment>
        {
            new Assignment { CourseId = courses[0].CourseId, Title = "Algebra Problem Set 1", Description = "Solve 20 linear equations", DueDate = DateTime.UtcNow.AddDays(7), MaxScore = 100, IsPublished = true },
            new Assignment { CourseId = courses[0].CourseId, Title = "Algebra Quiz 1", Description = "Chapter 1 quiz", DueDate = DateTime.UtcNow.AddDays(14), MaxScore = 50, IsPublished = true },
            new Assignment { CourseId = courses[1].CourseId, Title = "Physics Lab Report 1", Description = "Motion experiment report", DueDate = DateTime.UtcNow.AddDays(10), MaxScore = 100, IsPublished = true },
            new Assignment { CourseId = courses[3].CourseId, Title = "Hello World Program", Description = "Create your first program", DueDate = DateTime.UtcNow.AddDays(5), MaxScore = 50, IsPublished = true },
            new Assignment { CourseId = courses[3].CourseId, Title = "Calculator Project", Description = "Build a simple calculator", DueDate = DateTime.UtcNow.AddDays(21), MaxScore = 100, IsPublished = false }
        };
        context.Assignments.AddRange(assignments);
        context.SaveChanges();
    }

    // Seed Schedules
    if (!context.Schedules.Any())
    {
        var sessions = context.ClassSessions.ToList();
        var schedules = new List<Schedule>
        {
            new Schedule { ClassSessionId = sessions[0].Id, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), Room = "Room 101", EffectiveFrom = DateTime.UtcNow.AddDays(-30) },
            new Schedule { ClassSessionId = sessions[2].Id, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(12, 30, 0), Room = "Lab A", EffectiveFrom = DateTime.UtcNow.AddDays(-30) },
            new Schedule { ClassSessionId = sessions[4].Id, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 30, 0), Room = "Room 202", EffectiveFrom = DateTime.UtcNow.AddDays(-30) },
            new Schedule { ClassSessionId = sessions[6].Id, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(14, 0, 0), EndTime = new TimeSpan(15, 30, 0), Room = "Computer Lab 1", EffectiveFrom = DateTime.UtcNow.AddDays(-30) },
            new Schedule { ClassSessionId = sessions[8].Id, DayOfWeek = DayOfWeek.Thursday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(11, 30, 0), Room = "Room 105", EffectiveFrom = DateTime.UtcNow.AddDays(-30) }
        };
        context.Schedules.AddRange(schedules);
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
