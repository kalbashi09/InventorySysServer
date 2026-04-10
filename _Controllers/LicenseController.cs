using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; // Added for IConfiguration
using Services;
using Models.dto;

namespace Controllers;



[ApiController]
[Route("api/license")] // Publicly accessible for users to start trials
public class LicenseController : ControllerBase
{
    private readonly LicenseService _licenseService;

    public LicenseController(LicenseService licenseService)
    {
        _licenseService = licenseService;
    }

    [HttpPost("activate-trial")]
    public async Task<IActionResult> ActivateTrial([FromBody] TrialActivationDto dto)
    {
        // "Tell it like it is": Use the exact property name from your DTO (Hwid or HardwareId)
        var result = await _licenseService.ActivateTrial(dto.UserId, dto.HardwareId);
        
        if (result.StartsWith("Success")) return Ok(new { message = result });
        return BadRequest(new { message = result });
    }
}