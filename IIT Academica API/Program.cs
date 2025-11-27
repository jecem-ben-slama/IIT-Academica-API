using IIT_Academica_API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
// Add using statements for your services/repositories if not in the same file
// using YourNamespace.Services; 
// using YourNamespace.Repositories; 


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// FIX 1: Use IdentityUser<int> (ApplicationUser already inherits from it)
// FIX 2: Explicitly specify IdentityRole<int> for the Role type
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedAccount = false;
})
// FIX 3: AddEntityFrameworkStores must also use the explicit Role type
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

// JWT Bearer Configuration (No change needed here)
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

// Database Seeding and Role Initialization
using (var scope = app.Services.CreateScope())
{
    // FIX 4: Use RoleManager<IdentityRole<int>> (Explicit int key)
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Ensures the database exists and applies the migration
    context.Database.Migrate();

    var roles = new[] { "Admin", "Teacher", "Student" };
    foreach (var role in roles)
    {
        // FIX 5: Use IdentityRole<int>(role) when creating the new role object
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(role));
        }
    }

    // FIX 6: UserManager<ApplicationUser> is fine, as ApplicationUser is IdentityUser<int>
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var adminEmail = "admin@iit.edu";

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new ApplicationUser
        {
            Email = adminEmail,
            // Set the new properties instead of the old 'FullName'
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();