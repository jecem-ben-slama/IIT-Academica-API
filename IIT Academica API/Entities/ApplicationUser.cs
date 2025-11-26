using IIT_Academica_API.Entities;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

public class ApplicationUser : IdentityUser<int>
{

    public string? Name { get;  set; }
    public string? LastName { get; set; }

    public ICollection<Subject> TaughtSubjects { get; set; }
    public ICollection<Enrollment>? Enrollments { get; set; }
    public ICollection<AttendanceRecord>? AttendanceRecords { get; set; }
}