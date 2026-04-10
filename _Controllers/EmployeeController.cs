using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data;
using Models.Entities;

namespace Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeeController : ControllerBase
{
    private readonly InventoryDbContext _db;

    public EmployeeController(InventoryDbContext db)
    {
        _db = db;
    }

    // GET: api/employees/employer/{employerId}
    // This is the "Full Blast" filter to see everyone in the shop
    [HttpGet("employer/{employerId}")]
    public async Task<IActionResult> GetEmployeesByEmployer(string employerId)
    {
        // 1. Logic Check: Does the Employer even exist?
        var employerExists = await _db.Users.AnyAsync(u => u.Id == employerId && u.EmployerId == null);
        if (!employerExists) return NotFound("Employer not found.");

        // 2. Fetch the cluster
        var employees = await _db.Users
            .Where(u => u.EmployerId == employerId)
            .Select(u => new 
            {
                u.Id,
                u.FullName,
                u.Email,
                u.Birthdate,
                JoinedDate = EF.Property<DateTime>(u, "CreatedAt") // If you have a timestamp
            })
            .ToListAsync();

        return Ok(employees);
    }
}