using Microsoft.AspNetCore.Mvc;
using Models.dto;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")] // This makes the URL: api/UserStartup
public class UserStartupController : ControllerBase
{
    private readonly UserStartupService _startupService;

    public UserStartupController(UserStartupService startupService)
    {
        _startupService = startupService;
    }

    [HttpPost("initialize")]
    public async Task<IActionResult> Initialize([FromBody] UserStartupDto request)
    {
        // 1. Validation: "Tell it like it is"
        if (string.IsNullOrEmpty(request.UserId)) 
            return BadRequest("UserId is required.");

        // FullName is now REQUIRED for our AppUser entity
        if (string.IsNullOrEmpty(request.FullName)) 
            return BadRequest("Full Name is required.");

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
            return BadRequest("Password must be at least 8 characters long.");

        // 2. Pass everything to the service
        // Notice we are passing FullName, Birthdate, and SpecialCode now!
        var result = await _startupService.InitializeNewUser(
            request.UserId, 
            request.Email, 
            request.Password,
            request.FullName,     // Pass from DTO
            request.Birthdate,    // Pass from DTO
            request.SpecialCode,  // Pass from DTO
            request.Categories
        );

        return Ok(new { message = result });
    }
}