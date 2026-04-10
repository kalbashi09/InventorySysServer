using Microsoft.AspNetCore.Mvc;
using Models.dto;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserStartupController : ControllerBase
{
    private readonly UserStartupService _startupService;

    public UserStartupController(UserStartupService startupService)
    {
        _startupService = startupService;
    }

    // STEP 1: Register the Employer Account
    [HttpPost("initialize")]
    public async Task<IActionResult> Initialize([FromBody] UserStartupDto request)
    {
        // Validation: No sugar-coating. If it's empty, it's a fail.
        if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Email)) 
            return BadRequest("UserId and Email are required.");

        if (string.IsNullOrEmpty(request.FullName)) 
            return BadRequest("Full Name is required.");

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
            return BadRequest("Password must be at least 8 characters long.");

        // Call Part 1 of the Service
        var result = await _startupService.InitializeNewUser(
            request.UserId, 
            request.Email, 
            request.Password,
            request.FullName,
            request.Birthdate
        );

        if (!result.Success) return BadRequest(result.Message);

        return Ok(result);
    }

    // STEP 2: The "Setup Wizard" for Categories
    [HttpPost("setup-categories")]
    public async Task<IActionResult> SetupCategories([FromBody] CategorySetupRequestDto request)
    {
        if (string.IsNullOrEmpty(request.UserId)) 
            return BadRequest("UserId is required to link categories.");

        if (request.Categories == null || !request.Categories.Any())
            return BadRequest("Please select at least one category.");

        var result = await _startupService.SetupShopCategories(request.UserId, request.Categories);

        if (!result.Success) return BadRequest(result.Message);

        return Ok(result);
    }

    // EMPLOYEE JOIN: Remains a single-step process
    [HttpPost("join-employee")]
    public async Task<IActionResult> JoinEmployee([FromBody] EmployeeJoinDto request)
    {
        if (string.IsNullOrEmpty(request.SpecialCode)) 
            return BadRequest("Employer Special Code is required.");

        try
        {
            var result = await _startupService.OnboardEmployee(request);
            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}