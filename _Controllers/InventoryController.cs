using Microsoft.AspNetCore.Mvc;
using Models.dto;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryService _inventoryService;
    private readonly LicenseService _licenseService;

    public InventoryController(InventoryService inventoryService, LicenseService licenseService)
    {
        _inventoryService = inventoryService;
        _licenseService = licenseService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterItem([FromBody] InventoryEntryDto request)
    {
        if (request == null) return BadRequest("Invalid request payload.");
        
        try 
        {
            if (!await _licenseService.CanAccessAccount(request.UserId))
                return StatusCode(403, "Access denied. Shop license is inactive.");

            var result = await _inventoryService.RegisterScannedItem(request);
            
            if (!result.Success || result.Data == null) 
                return BadRequest(new { message = result.Message });

            // INNOVATIVE: Map the Entity back to a DTO before returning
            // This ensures the mobile app gets a clean "Success" object
            var displayData = new InventoryDisplayDto
            {
                ItemId = result.Data!.Id, 
                ItemName = result.Data.ItemName,
                QuantityDisplay = $"{result.Data.Quantity} {result.Data.QuantityType}",
                BatchLocation = result.Data.BatchName,
                CategoryName = result.Data.UserCategory?.Name ?? "", 
                TypeName = result.Data.InventoryType?.TypeName ?? "",
                IsExpired = result.Data.IsExpired
            };

            return CreatedAtAction(nameof(GetItems), 
                new { userId = request.UserId }, 
                ServiceResult<InventoryDisplayDto>.Ok(displayData, result.Message));
        }
        catch (Exception ex) 
        { 
            return StatusCode(500, new { message = "Internal error.", detail = ex.Message }); 
        }
    }

    [HttpGet("{userId}/items")]
    public async Task<IActionResult> GetItems(string userId, [FromQuery] int? categoryId, [FromQuery] int? typeId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest("UserId is required.");

        try 
        {
            if (!await _licenseService.CanAccessAccount(userId))
                return StatusCode(403, "Access denied. Shop license is invalid.");

            var items = await _inventoryService.GetUserInventory(userId, categoryId, typeId);
            return Ok(items);
        }
        catch (Exception ex) { return StatusCode(500, $"Internal server error: {ex.Message}"); }
    }

    // NEW ENDPOINT: Fetch Expired Items specifically
    [HttpGet("{userId}/expired")]
    public async Task<IActionResult> GetExpiredItems(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest("UserId is required.");

        try 
        {
            if (!await _licenseService.CanAccessAccount(userId))
                return StatusCode(403, "Access denied. Shop license is invalid.");

            var result = await _inventoryService.GetExpiredStock(userId);
            return Ok(result);
        }
        catch (Exception ex) { return StatusCode(500, $"Internal server error: {ex.Message}"); }
    }

    [HttpPost("purge")]
    public async Task<IActionResult> Purge([FromBody] List<Guid> ids, [FromQuery] string userId)
    {
        // 1. Validation
        if (ids == null || !ids.Any()) return BadRequest("No items selected.");
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest("UserId is required.");

        try 
        {
            // 2. SECURITY CHECK: Only allow the Boss to purge
            // We use your existing LicenseService or a direct DB check
            var isAuthorized = await _inventoryService.IsUserAdmin(userId); 
            if (!isAuthorized) 
            {
                return StatusCode(403, "Access denied. Only shop owners can purge inventory.");
            }

            // 3. Action
            await _inventoryService.PurgeItems(ids);
            
            return Ok(new { message = $"{ids.Count} items successfully removed from stock." });
        }
        catch (Exception ex) 
        { 
            return StatusCode(500, new { message = "Error during purge.", detail = ex.Message }); 
        }
    }
}