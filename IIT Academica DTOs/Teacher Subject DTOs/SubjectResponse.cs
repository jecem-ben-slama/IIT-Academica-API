// DTOs/TeacherSubjectResponse.cs
public class SubjectResponse
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string RegistrationCode { get; set; }
    public string TeacherName { get; set; } // Display name from ApplicationUser
    public int EnrollmentCount { get; set; } = 0; // Starts at 0
}