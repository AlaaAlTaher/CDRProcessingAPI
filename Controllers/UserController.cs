using Microsoft.AspNetCore.Mvc;
using CDRProcessingAPI.Data;
using CDRProcessingAPI.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CDRProcessingAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly CDRDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(CDRDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve all users without their IDs.
        /// </summary>
        /// <returns>A list of users without their IDs.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllUsers()
        {
            _logger.LogInformation("Retrieving all users without IDs.");
            var users = await _context.Users
                .Select(u => new
                {
                    Name = u.Name,
                    MSISDN = u.MSISDN
                })
                .ToListAsync();
            return Ok(users);
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="user">The user to create.</param>
        /// <returns>Created user without ID.</returns>
        [HttpPost]
        public async Task<ActionResult<object>> CreateUser(User user)
        {
            _logger.LogInformation("Attempting to create a new user with MSISDN: {MSISDN}", user.MSISDN);

            if (await _context.Users.AnyAsync(u => u.MSISDN == user.MSISDN))
            {
                _logger.LogWarning("User creation failed. MSISDN {MSISDN} already exists.", user.MSISDN);
                return BadRequest("MSISDN must be unique. This MSISDN already exists.");
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User created successfully with MSISDN: {MSISDN}", user.MSISDN);
            return CreatedAtAction(nameof(CreateUser), new { id = user.Id }, new
            {
                Name = user.Name,
                MSISDN = user.MSISDN
            });
        }
    }
}
