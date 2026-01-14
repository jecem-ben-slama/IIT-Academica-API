using Microsoft.EntityFrameworkCore;
using IIT_Academica_API.Entities;

public class EnrollmentRepositoryTests
{
    private ApplicationDbContext GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetEnrollmentsByStudentId_IncludesSubjectAndTeacher()
    {
        // Arrange
        using var context = GetDatabaseContext();
        var repository = new EnrollmentRepository(context);

        // Création du Teacher en tant qu'ApplicationUser
        var teacher = new ApplicationUser
        {
            Id = 1,
            UserName = "teacher@test.com",
            Name = "Jean",
            LastName = "Dupont"
        };

        var subject = new Subject
        {
            Id = 1,
            Title = "C#",
            Teacher = teacher,
            RegistrationCode = "CSHARP-001"
        };

        context.Enrollments.Add(new Enrollment { StudentId = 10, Subject = subject });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetEnrollmentsByStudentIdAsync(10);

        // Assert
        var enrollment = Assert.Single(result);
        Assert.NotNull(enrollment.Subject);
        Assert.Equal("Jean", enrollment.Subject.Teacher?.Name); // Vérifie que l'Include du Teacher fonctionne
    }

    [Fact]
    public async Task HasActiveEnrollmentsForSubject_WhenEnrolled_ReturnsTrue()
    {
        // Arrange (Technique : Test de Condition)
        using var context = GetDatabaseContext();
        var repository = new EnrollmentRepository(context);

        context.Enrollments.Add(new Enrollment { StudentId = 1, SubjectId = 5 });
        await context.SaveChangesAsync();

        // Act
        var result = await repository.HasActiveEnrollmentsForSubject(5);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AddEnrollmentAsync_ShouldSaveCorrectly()
    {
        // Arrange
        using var context = GetDatabaseContext();
        var repository = new EnrollmentRepository(context);
        var newEnroll = new Enrollment { StudentId = 1, SubjectId = 1 };

        // Act
        await repository.AddEnrollmentAsync(newEnroll);
        await context.SaveChangesAsync(); // Important : On simule l'unité de travail

        // Assert
        var dbEnroll = await context.Enrollments.FirstOrDefaultAsync();
        Assert.NotNull(dbEnroll);
        Assert.Equal(1, dbEnroll.StudentId);
    }
}