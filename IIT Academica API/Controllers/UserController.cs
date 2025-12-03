using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using IIT_Academica_DTOs;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public UserController(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
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

        if (user == null) return NotFound("User not found.");

        user.Name = model.Name!;
        user.LastName = model.LastName!;
        user.Email = model.Email;
        user.UserName = model.Email;

        var result = await _userRepository.UpdateAsync(user);

        if (result.Succeeded)
            return Ok(new UserReadDto { Id = user.Id, Email = user.Email, Name = user.Name!, LastName = user.LastName! });

        return BadRequest(result.Errors);
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
}