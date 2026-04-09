using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly LicenseService _licenseService;
    // In a real app, keep this in appsettings.json or an Environment Variable
    private const string AdminApiKey = "Talisay_Admin_Secret_2026_!@#"; 

    public AdminController(LicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    [HttpPatch("activate-pl/{userId}")]
    public async Task<IActionResult> ActivatePermanent(string userId, [FromHeader(Name = "X-Admin-Key")] string providedKey)
    {
        // 1. Tell it like it is: Security check
        if (providedKey != AdminApiKey)
        {
            return Unauthorized("Invalid Admin Key. Access Denied.");
        }

        // 2. Process the upgrade
        var result = await _licenseService.ActivatePermanentLicense(userId);

        if (!result) return NotFound("User not found or License row missing.");

        return Ok(new { message = $"User {userId} is now a Permanent Member!" });
    }
}