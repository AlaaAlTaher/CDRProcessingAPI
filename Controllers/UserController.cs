using Microsoft.AspNetCore.Mvc;
using CDRProcessingAPI.Data;
using CDRProcessingAPI.Models;
using Microsoft.EntityFrameworkCore;

using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly CDRDbContext _context;

    public UserController(CDRDbContext context)
    {
        _context = context;
    }

    // POST: api/User    for creating users
    [HttpPost("Ceate User")]
    public async Task<ActionResult<object>> CreateUser(User user)
    {
        // Check if MSISDN is unique
        if (await _context.Users.AnyAsync(u => u.MSISDN == user.MSISDN))
        {
            return BadRequest("MSISDN must be unique. This MSISDN already exists.");
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(CreateUser), new { id = user.Id }, new
        {
            Name = user.Name,
            MSISDN = user.MSISDN
        });
    }



    // getting all users info 
    [HttpGet("Get All Users")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
    {
        var users = await _context.Users
            .Select(u => new
            {
                Name = u.Name,
                MSISDN = u.MSISDN
            })
            .ToListAsync();

        return Ok(users);
    }

}
