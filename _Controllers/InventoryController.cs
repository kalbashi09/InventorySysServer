using Microsoft.AspNetCore.Mvc;
using Models.dto;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryService _inventoryService;

    public InventoryController(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterItem([FromBody] InventoryEntryDto request)
    {
        // 1. Tell it like it is: Guard your data
        if (request == null) return BadRequest("Invalid request payload.");
        
        // 2. Validate essential fields before hitting the service
        if (request.Quantity <= 0) 
            return BadRequest("Quantity must be greater than zero.");

        try 
        {
            var result = await _inventoryService.RegisterScannedItem(request);
            
            // This is much cleaner than string checking
            return CreatedAtAction(nameof(GetItems), new { userId = request.UserId }, new { message = "Item registered successfully", data = result });
        }
        catch (KeyNotFoundException ex)
        {
            // Example: Category or User doesn't exist
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected error occurred.", detail = ex.Message });
        }
    }

    [HttpGet("{userId}/items")]
    public async Task<IActionResult> GetItems(string userId, [FromQuery] int? categoryId, [FromQuery] int? typeId)
    {
        try 
        {
            // 1. "Tell it like it is" Validation
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId is required to fetch inventory.");

            // 2. Execute the dual-filter query
            var items = await _inventoryService.GetUserInventory(userId, categoryId, typeId);

            // 3. Return a clean result (Empty list is better than a 404 for search results)
            return Ok(items);
        }
        catch (Exception ex)
        {
            // Log the error (In development, this helps you debug on your laptop)
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}