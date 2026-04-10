using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; // Added for IConfiguration
using Services;
using Models.dto;

namespace Controllers;

[ApiController]
[Route("api/admin")] // Everything here stays behind the secret key
public class AdminController : ControllerBase
{
    private readonly LicenseService _licenseService;
    private readonly IConfiguration _config;

    public AdminController(LicenseService licenseService, IConfiguration config)
    {
        _licenseService = licenseService;
        _config = config;
    }

    [HttpPatch("activate-permanent")]
    public async Task<IActionResult> ActivatePermanent([FromBody] AdminLicenseUpdateDto dto, [FromHeader(Name = "X-Admin-Key")] string providedKey)
    {
        var adminApiKey = _config["AdminSettings:SecretKey"];
        if (providedKey != adminApiKey) return Unauthorized("Access Denied.");

        var result = await _licenseService.ActivatePermanentLicense(dto.UserId);
        if (!result) return NotFound("User/License not found.");

        return Ok(new { message = $"User {dto.UserId} upgraded to Permanent." });
    }
}
