using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using IIT_Academica_API.Entities;
using IIT_Academica_API.Models.DTOs;
using IIT_Academica_DTOs.Enrollment_DTOs;

public class EnrollmentControllerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly EnrollmentController _controller;
    private const int MockStudentId = 123;

    public EnrollmentControllerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _controller = new EnrollmentController(_mockUnitOfWork.Object);

        // --- ISTQB: Setting up the Test Environment (Mocking the User) ---
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, MockStudentId.ToString()),
            new Claim(ClaimTypes.Role, "Student")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task Enroll_ValidRequest_ReturnsCreated()
    {
        // 1. Arrange (ISTQB: Positive Testing / Happy Path)
        var dto = new EnrollmentRequestDto { SubjectId = 1, RegistrationCode = "MATH101" };
        var subject = new Subject { Id = 1, RegistrationCode = "MATH101", Title = "Calculus" };

        _mockUnitOfWork.Setup(u => u.Subjects.GetByIdAsync(dto.SubjectId)).ReturnsAsync(subject);
        _mockUnitOfWork.Setup(u => u.Enrollments.IsStudentAlreadyEnrolledAsync(MockStudentId, subject.Id)).ReturnsAsync(false);

        // 2. Act
        var result = await _controller.Enroll(dto);

        // 3. Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var responseDto = Assert.IsType<EnrollmentResponseDto>(createdResult.Value);
        Assert.Equal(subject.Title, responseDto.SubjectTitle);
        _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task Enroll_InvalidRegistrationCode_ReturnsBadRequest()
    {
        // 1. Arrange (ISTQB: Negative Testing / Boundary Value)
        var dto = new EnrollmentRequestDto { SubjectId = 1, RegistrationCode = "WRONG_CODE" };
        var subject = new Subject { Id = 1, RegistrationCode = "CORRECT_CODE" };

        _mockUnitOfWork.Setup(u => u.Subjects.GetByIdAsync(dto.SubjectId)).ReturnsAsync(subject);

        // 2. Act
        var result = await _controller.Enroll(dto);

        // 3. Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Invalid registration code", badRequest.Value.ToString());
    }

    [Fact]
    public async Task Enroll_SubjectDoesNotExist_ReturnsBadRequest()
    {
        // 1. Arrange (ISTQB: Error Guessing)
        var dto = new EnrollmentRequestDto { SubjectId = 999 };
        _mockUnitOfWork.Setup(u => u.Subjects.GetByIdAsync(999)).ReturnsAsync((Subject)null);

        // 2. Act
        var result = await _controller.Enroll(dto);

        // 3. Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DropCourse_Successful_ReturnsNoContent()
    {
        // 1. Arrange
        int enrollmentId = 50;
        _mockUnitOfWork.Setup(u => u.Enrollments.DeleteEnrollmentByStudentAndIdAsync(enrollmentId, MockStudentId))
                       .ReturnsAsync(true);

        // 2. Act
        var result = await _controller.DropCourse(enrollmentId);

        // 3. Assert
        Assert.IsType<NoContentResult>(result);
        _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }
}