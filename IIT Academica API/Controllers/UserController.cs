using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        var user = new ApplicationUser { UserName = model.Email, Name = model.Name, LastName =model.LastName,Email = model.Email };

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
                // FIX: Map the ApplicationUser properties to the DTO properties
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
            userDtos.Add(new UserReadDto { Id = user.Id, Email = user.Email!, Name = user.Name!,LastName=user.LastName!, Role = roles.FirstOrDefault()! });
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
    public async Task<IActionResult> DeleteUser(UserDeleteDto model)
    {
        var user = await _userRepository.GetByIdAsync(model.Id);

        if (user == null) return NotFound("User not found.");

        var result = await _userRepository.DeleteAsync(user);

        if (result.Succeeded) return NoContent();

        return BadRequest(result.Errors);
    }
}