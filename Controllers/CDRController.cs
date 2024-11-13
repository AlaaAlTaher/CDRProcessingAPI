using Microsoft.AspNetCore.Mvc;
using CDRProcessingAPI.Data;
using CDRProcessingAPI.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CDRProcessingAPI.Controllers
{
    [Route("api/cdrs")]
    [ApiController]
    public class CDRController : ControllerBase
    {
        private readonly CDRDbContext _context;
        private readonly ILogger<CDRController> _logger;

        public CDRController(CDRDbContext context, ILogger<CDRController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve all call data records (CDRs).
        /// </summary>
        /// <returns>A list of all CDRs.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CDR>>> GetAllCDRs()
        {
            _logger.LogInformation("Retrieving all CDRs.");
            return await _context.CDRs.ToListAsync();
        }

        /// <summary>
        /// Retrieve a call data record (CDR) by ID.
        /// </summary>
        /// <param name="id">The ID of the CDR.</param>
        /// <returns>The requested CDR.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CDR>> GetCDRById(int id)
        {
            _logger.LogInformation("Retrieving CDR with ID: {Id}", id);
            var cdr = await _context.CDRs.FindAsync(id);

            if (cdr == null)
            {
                _logger.LogWarning("CDR with ID: {Id} not found.", id);
                return NotFound();
            }

            return cdr;
        }

        /// <summary>
        /// Create a new call data record (CDR).
        /// </summary>
        /// <param name="cdr">The CDR to create.</param>
        /// <returns>Created CDR without ID.</returns>
        [HttpPost]
        public async Task<ActionResult<object>> CreateCDR(CDR cdr)
        {
            _logger.LogInformation("Attempting to create a new CDR for caller {CallerMSISDN} and receiver {ReceiverMSISDN}.", cdr.CallerMSISDN, cdr.ReceiverMSISDN);

            if (cdr.CallerMSISDN == cdr.ReceiverMSISDN)
            {
                _logger.LogWarning("CDR creation failed. Caller MSISDN and Receiver MSISDN are the same.");
                return BadRequest("Caller MSISDN and Receiver MSISDN must be different.");
            }

            if (!new[] { "local", "long-distance", "international" }.Contains(cdr.CallType.ToLower()))
            {
                _logger.LogWarning("CDR creation failed. Invalid CallType: {CallType}.", cdr.CallType);
                return BadRequest("Invalid CallType. Allowed values are: local, long-distance, international.");
            }

            _context.CDRs.Add(cdr);
            await _context.SaveChangesAsync();

            _logger.LogInformation("CDR created successfully for caller {CallerMSISDN} and receiver {ReceiverMSISDN}.", cdr.CallerMSISDN, cdr.ReceiverMSISDN);
            return CreatedAtAction(nameof(CreateCDR), new { id = cdr.Id }, new
            {
                CallerMSISDN = cdr.CallerMSISDN,
                ReceiverMSISDN = cdr.ReceiverMSISDN,
                Duration = cdr.Duration,
                Timestamp = cdr.Timestamp,
                CallType = cdr.CallType
            });
        }

        /// <summary>
        /// Calculate call charges for a CDR by ID.
        /// </summary>
        /// <param name="id">The ID of the CDR.</param>
        /// <returns>Total charge for the call.</returns>
        [HttpGet("calculate-charge/{id}")]
        public async Task<ActionResult<decimal>> CalculateCharge(int id)
        {
            _logger.LogInformation("Calculating charge for CDR with ID: {Id}", id);

            var cdr = await _context.CDRs.FindAsync(id);
            if (cdr == null)
            {
                _logger.LogWarning("CDR with ID: {Id} not found for charge calculation.", id);
                return NotFound("CDR not found.");
            }

            decimal ratePerMinute = cdr.CallType.ToLower() switch
            {
                "local" => 0.05m,
                "long-distance" => 0.10m,
                "international" => 0.50m,
                _ => 0m
            };

            var durationInMinutes = Math.Ceiling(cdr.Duration / 60.0);
            var charge = (decimal)durationInMinutes * ratePerMinute;

            _logger.LogInformation("Charge for CDR with ID: {Id} calculated as {Charge}.", id, charge);
            return Ok(charge);
        }

        /// <summary>
        /// Retrieve a summary for a specific user based on their ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>Summary of total calls, duration, and charges for the user.</returns>
        [HttpGet("summary/{userId}")]
        public async Task<ActionResult> GetUserSummary(int userId)
        {
            _logger.LogInformation("Retrieving call summary for user with ID: {UserId}", userId);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID: {UserId} not found for summary.", userId);
                return NotFound("User not found");
            }

            var userCDRs = await _context.CDRs
                .Where(cdr => cdr.CallerMSISDN == user.MSISDN)
                .ToListAsync();

            var totalCalls = userCDRs.Count;
            var totalDuration = userCDRs.Sum(cdr => cdr.Duration);
            var totalCharges = userCDRs.Sum(cdr =>
            {
                decimal ratePerMinute = cdr.CallType.ToLower() switch
                {
                    "local" => 0.05m,
                    "long-distance" => 0.10m,
                    "international" => 0.50m,
                    _ => 0m
                };
                return (decimal)Math.Ceiling(cdr.Duration / 60.0) * ratePerMinute;
            });

            _logger.LogInformation("Summary for user ID: {UserId} retrieved.", userId);
            return Ok(new
            {
                TotalCalls = totalCalls,
                TotalDuration = totalDuration,
                TotalCharges = totalCharges
            });
        }
    }
}
