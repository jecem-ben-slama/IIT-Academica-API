using IIT_Academica_API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Net.Http; 

var builder = WebApplication.CreateBuilder(args);

// Define a policy name for clarity and consistency
var MyAllowDevelopmentOrigins = "_myAllowDevelopmentOrigins"; 

// 🛑 CORS FIX 1: Add CORS services with a simple development-friendly policy
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowDevelopmentOrigins,
        policy =>
        {
            // Allow requests from *any* origin on any port during development
            policy.AllowAnyOrigin() 
                  .AllowAnyHeader()  // Essential for allowing Authorization header (JWT)
                  .AllowAnyMethod(); // Essential for allowing POST/GET/DELETE requests
        });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity Configuration (using IdentityRole<int> for Role type)
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


// Assuming these service registrations are correct and their interfaces/implementations exist
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<ICourseMaterialRepository, CourseMaterialRepository>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// JWT Bearer Configuration
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

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 1. Static Files (Should be early)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Uploads")),
    RequestPath = "/Uploads"
});


// Database Seeding and Role Initialization (Moved to run once during startup)
using (var scope = app.Services.CreateScope())
{
    // ... Database seeding logic (unchanged)
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    context.Database.Migrate();

    var roles = new[] { "Admin", "Teacher", "Student" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(role));
        }
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var adminEmail = "admin@iit.edu";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new ApplicationUser
        {
            Email = adminEmail,
            UserName = adminEmail,
            Name = "System",
            LastName = "Admin"
        }; var password = builder.Configuration["AdminPassword"];

        var createResult = await userManager.CreateAsync(adminUser, password);
        if (createResult.Succeeded)
        {
            var persisted = await userManager.FindByEmailAsync(adminEmail);
            if (persisted != null)
            {
                await userManager.AddToRoleAsync(persisted, "Admin");
            }
        }
        else
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            Console.WriteLine($"Failed to create admin user: {errors}");
        }
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyAllowDevelopmentOrigins); 

// 3. Security Middleware
app.UseAuthentication();
app.UseAuthorization();


// 4. Controller Mapping
app.MapControllers();

app.Run();