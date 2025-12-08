using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using IIT_Academica_DTOs; // Assuming your DTOs are here
using IIT_Academica_API.Entities; // Assuming ApplicationUser is here
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities; // Necessary for URL encoding/decoding

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService; // <--- NEW INJECTION

    // Assuming a base URL for the client application's reset page
    private const string ClientResetUrl = "http://localhost:5148/user/reset-password"; 

    public UserController(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IEmailService emailService) // <--- NEW INJECTION
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _emailService = emailService; // <--- ASSIGNMENT
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new ApplicationUser { UserName = model.Email, Name = model.Name, LastName = model.LastName, Email = model.Email };

        var result = await _userRepository.RegisterAndAssignRoleAsync(user, model.Password, model.Role);

        if (result.Succeeded)
        {
            var roles = await _userRepository.GetUserRolesAsync(user);
            var token = _tokenService.CreateToken(user, roles);
            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "User registered successfully.",
                Token = token,
                UserId = user.Id,
                Role = roles.FirstOrDefault(),
                Name = user.Name,
                LastName = user.LastName,
                Errors = null
            });
        }

        var errors = result.Errors
            .Select(e => e.Description)
            .ToList();

        return BadRequest(new AuthResponseDto
        {
            Message = "Registration failed",
            IsSuccess = false,
            Errors = errors
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _userRepository.ValidateCredentialsAsync(model.Email, model.Password);

        if (user != null)
        {

            var roles = await _userRepository.GetUserRolesAsync(user);
            var token = _tokenService.CreateToken(user, roles);

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Login successful",
                Token = token,
                UserId = user.Id,
                Role = roles.FirstOrDefault()
            });
        }

        return Unauthorized(new AuthResponseDto { Message = "Invalid credentials." });
    }


    [HttpGet("GetAllUsers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userRepository.GetAllAsync();

        var userDtos = new List<UserReadDto>();
        foreach (var user in users)
        {
            var roles = await _userRepository.GetUserRolesAsync(user);
            userDtos.Add(new UserReadDto { Id = user.Id, Email = user.Email!, Name = user.Name!, LastName = user.LastName!, Role = roles.FirstOrDefault()! });
        }

        return Ok(userDtos);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound($"User with ID {id} not found.");

        var roles = await _userRepository.GetUserRolesAsync(user);

        return Ok(new UserReadDto { Id = user.Id, Email = user.Email!, Name = user.Name!, LastName = user.LastName!, Role = roles.FirstOrDefault()! });
    }


    [HttpPut("update")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto model)
{
    var user = await _userRepository.GetByIdAsync(model.Id);

    if (user == null)
        return NotFound("User not found.");

    // ---- Update basic user info ----
    user.Name = model.Name!;
    user.LastName = model.LastName!;
    user.Email = model.Email;
    user.UserName = model.Email;

    // ---- Update user data ----
    var result = await _userRepository.UpdateAsync(user);

    if (!result.Succeeded)
        return BadRequest(result.Errors);

    // ---- Update role if provided ----
    if (!string.IsNullOrWhiteSpace(model.role))
    {
        // Get the current role
        var currentRole = await _userRepository.GetUserRoleAsync(user);

        if (currentRole != model.role)
        {
            // Remove old role (if exists)
            if (currentRole != null)
                await _userRepository.RemoveFromRoleAsync(user, currentRole);

            // Add new role
            var addRoleResult = await _userRepository.AddToRoleAsync(user, model.role);

            if (!addRoleResult.Succeeded)
                return BadRequest(addRoleResult.Errors);
        }
    }

    // ---- Return updated user ----
    return Ok(new UserReadDto 
    { 
        Id = user.Id, 
        Email = user.Email, 
        Name = user.Name!, 
        LastName = user.LastName!, 
        Role = model.role!
    });
}


    [HttpDelete("delete")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteUser([FromBody] UserDeleteDto model)
{
    // The model binding will handle validation for the presence of the DTO
    if (model == null) return BadRequest("Invalid request body.");

    var user = await _userRepository.GetByIdAsync(model.Id);

    if (user == null) 
    {
        return NotFound($"User with ID {model.Id} not found.");
    }
    
    try
    {
        var result = await _userRepository.DeleteAsync(user);

        if (result.Succeeded)
        {
            return NoContent(); // 204 Success
        }
        else
        {
            // Handle Identity errors (e.g., if it uses Identity and has internal issues)
            return BadRequest(result.Errors);
        }
    }
    catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
    {
        // 1. Check the InnerException for signs of a Foreign Key Constraint violation.
        // This usually indicates that the user is linked to other records.
        
        // Note: The specific error check (InnerException message) is dependent on the database provider (SQL Server, PostgreSQL, etc.), 
        // but checking for DbUpdateException is the first step.
        
        // For a foreign key violation, return a 409 Conflict.
        // The object returned is what the client's UserService will deserialize.
        return Conflict(new {
            message = $"Cannot delete user '{user.Email}'. The user is currently linked to one or more records (e.g., courses, enrollments, grades). Please remove all associated records first.",
            errorCode = "ForeignKeyConstraint"
        });
    }
    catch (Exception ex)
    {
        // Catch any other unexpected server-side errors
        // Log the exception details here for debugging
        return StatusCode(500, new { 
            message = "An unexpected error occurred during user deletion.",
            details = ex.Message
        });
    }
}
 // --- NEW PASSWORD RESET ACTIONS ---
    
    // Step 1: Initiates the token generation and email sending
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 1. Check if the user exists
        var user = await _userRepository.GetByEmailAsync(model.Email);
        
        // SECURITY NOTE: To prevent user enumeration, we always return a success message
        // even if the user doesn't exist. This prevents attackers from guessing valid emails.
        if (user == null)
        {
            return Ok(new { Message = "If an account with that email exists, a password reset link has been sent." });
        }

        // 2. Generate the secure reset token
        var token = await _userRepository.GeneratePasswordResetTokenAsync(model.Email);
        if (string.IsNullOrEmpty(token))
        {
            return StatusCode(500, new { Message = "Could not generate reset token." });
        }
        
        // 3. Encode the token for safe transmission in a URL
        // Identity tokens often contain characters that are invalid in URLs.
        var encodedToken = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(token));

        // 4. Construct the reset link for the client application
        // The client must know how to parse the email and token from this URL
        var resetLink = $"{ClientResetUrl}?email={model.Email}&token={encodedToken}";

        // 5. Create the email content
        var emailBody = $@"
            <h1>Password Reset Request</h1>
            <p>You requested a password reset for your IIT Academica account ({model.Email}).</p>
            <p>Please click the link below to reset your password:</p>
            <p><a href='{resetLink}'>Reset My Password</a></p>
            <p>If you did not request this, please ignore this email.</p>";

        // 6. Send the email
        try
        {
            await _emailService.SendEmailAsync(
                model.Email,
                "IIT Academica - Password Reset Request",
                emailBody);
        }
        catch (Exception ex)
        {
            // Log the exception (ex) here. Don't expose internal server errors to the client.
            return StatusCode(500, new { Message = "Failed to send reset email. Please try again later." });
        }

        // Return success message regardless of whether the email was sent or if the user existed (see security note)
        return Ok(new { Message = "If an account with that email exists, a password reset link has been sent.",token=encodedToken });
    }
    
    // Step 2: Validates the token and sets the new password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 1. Find the user
        var user = await _userRepository.GetByEmailAsync(model.Email);
        
        if (user == null)
        {
            // Always return a generic failure message to prevent user enumeration
            return BadRequest(new { Message = "Password reset failed. Please ensure the link is correct." });
        }

        // 2. Decode the token before passing it to the Identity service
        string decodedToken;
        try
        {
            var tokenBytes = WebEncoders.Base64UrlDecode(model.Token);
            decodedToken = System.Text.Encoding.UTF8.GetString(tokenBytes);
        }
        catch
        {
            return BadRequest(new { Message = "Invalid token format." });
        }

        // 3. Call the repository to reset the password
        var result = await _userRepository.ResetPasswordAsync(model.Email, decodedToken, model.NewPassword);

        if (result.Succeeded)
        {
            return Ok(new { Message = "Password has been successfully reset. You can now log in." });
        }

        // Handle Identity errors (token expired, token invalid, password requirements not met)
        var errors = result.Errors
            .Select(e => e.Description)
            .ToList();

        return BadRequest(new { Message = "Password reset failed.", Errors = errors });
    }
}