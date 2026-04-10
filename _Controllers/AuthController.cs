using Models.dto;
using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; // Added for IConfiguration
using Services;

namespace Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly LoginService _loginService;

    public AuthController(LoginService loginService)
    {
        _loginService = loginService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _loginService.Authenticate(request);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.Message });
        }

        return Ok(result);
    }
}
