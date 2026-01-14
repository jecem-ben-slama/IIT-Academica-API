using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IIT_Academica_DTOs;
using Microsoft.AspNetCore.WebUtilities;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private const string ClientResetUrl = "http://localhost:5148/user/reset-password";

    public UserController(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    //^ Register
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
    //^ Login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        // On lance la validation des identifiants
        var user = await _userRepository.ValidateCredentialsAsync(model.Email, model.Password);

        if (user != null)
        {
            // OPTIMISATION : On récupère les rôles et on prépare d'autres données en parallèle si besoin
            // Ici, on gagne du temps en évitant les attentes inutiles
            var roles = await _userRepository.GetUserRolesAsync(user);

            // Création du token (Opération CPU pure, très rapide)
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

    //^ Get All
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
    //^ Get By Id
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound($"User with ID {id} not found.");

        var roles = await _userRepository.GetUserRolesAsync(user);

        return Ok(new UserReadDto { Id = user.Id, Email = user.Email!, Name = user.Name!, LastName = user.LastName!, Role = roles.FirstOrDefault()! });
    }

    //^ Update
    [HttpPut("update")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto model)
    {
        var user = await _userRepository.GetByIdAsync(model.Id);

        if (user == null)
            return NotFound("User not found.");

        user.Name = model.Name!;
        user.LastName = model.LastName!;
        user.Email = model.Email;
        user.UserName = model.Email;

        var result = await _userRepository.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        if (!string.IsNullOrWhiteSpace(model.role))
        {
            var currentRole = await _userRepository.GetUserRoleAsync(user);

            if (currentRole != model.role)
            {
                if (currentRole != null)
                    await _userRepository.RemoveFromRoleAsync(user, currentRole);

                var addRoleResult = await _userRepository.AddToRoleAsync(user, model.role);

                if (!addRoleResult.Succeeded)
                    return BadRequest(addRoleResult.Errors);
            }
        }

        return Ok(new UserReadDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name!,
            LastName = user.LastName!,
            Role = model.role!
        });
    }

    //^ Delete
    [HttpDelete("delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser([FromBody] UserDeleteDto model)
    {
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
                return NoContent();
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            return Conflict(new
            {
                exception = $"{ex.Message}",
                message = $"Cannot delete user '{user.Email}'. The user is currently linked to one or more records (e.g., courses, enrollments, grades). Please remove all associated records first.",
                errorCode = "ForeignKeyConstraint"
            });
        }
        catch (Exception ex)
        {

            return StatusCode(500, new
            {
                message = "An unexpected error occurred during user deletion.",
                details = ex.Message
            });
        }
    }
    //^ Forgot Password
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgetPasswordDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userRepository.GetByEmailAsync(model.Email);


        if (user == null)
        {
            return Ok(new { Message = "If an account with that email exists, a password reset link has been sent." });
        }

        var token = await _userRepository.GeneratePasswordResetTokenAsync(model.Email);
        if (string.IsNullOrEmpty(token))
        {
            return StatusCode(500, new { Message = "Could not generate reset token." });
        }
        var encodedToken = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(token));

        var resetLink = $"{ClientResetUrl}?email={model.Email}&token={encodedToken}";

        var emailBody = $@"
            <h1>Password Reset Request</h1>
            <p>You requested a password reset for your IIT Academica account ({model.Email}).</p>
            <p>Please click the link below to reset your password:</p>
            <p><a href='{resetLink}'>Reset My Password</a></p>
            <p>If you did not request this, please ignore this email.</p>";

        try
        {
            await _emailService.SendEmailAsync(
                model.Email,
                "IIT Academica - Password Reset Request",
                emailBody);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Message = "Failed to send reset email. Please try again later.",
                details = ex.Message
            });
        }

        return Ok(new { Message = "If an account with that email exists, a password reset link has been sent.", token = encodedToken });
    }
    //^ Reset Password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userRepository.GetByEmailAsync(model.Email);

        if (user == null)
        {
            return BadRequest(new { Message = "Password reset failed. Please ensure the link is correct." });
        }

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

        var result = await _userRepository.ResetPasswordAsync(model.Email, decodedToken, model.NewPassword);

        if (result.Succeeded)
        {
            return Ok(new { Message = "Password has been successfully reset. You can now log in." });
        }

        var errors = result.Errors
            .Select(e => e.Description)
            .ToList();

        return BadRequest(new { Message = "Password reset failed.", Errors = errors });
    }
}
