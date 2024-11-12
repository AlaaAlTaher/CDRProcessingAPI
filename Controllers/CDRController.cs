using CDRProcessingAPI.Data;
using CDRProcessingAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



[Route("api/[controller]")]
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

    // Example CRUD methods here (e.g., GET, POST, PUT, DELETE)

    [HttpGet("Get All CRDS")] // get: api/crd
    public async Task<ActionResult<IEnumerable<CDR>>> GetCDRs()
    {
        return await _context.CDRs.ToListAsync();
    }


    [HttpGet("Get CRD by ID")]    // GET: api/CDR/{id}
    public async Task<ActionResult<CDR>> GetCDR(int id)
    {
        var cdr = await _context.CDRs.FindAsync(id);

        if (cdr == null)
        {
            return NotFound();
        }

        return cdr;
    }


    // POST: api/CDR
    /* [HttpPost]
     public async Task<ActionResult<CDR>> CreateCDR(CDR cdr)
     {
         _context.CDRs.Add(cdr);
         await _context.SaveChangesAsync();

         return CreatedAtAction(nameof(GetCDR), new { id = cdr.Id }, cdr);
     }
    */ //old before validation

    [HttpPost("Create CRD")]
    public async Task<ActionResult<object>> CreateCDR(CDR cdr)
    {
        // Log the start of CDR creation
        _logger.LogInformation($"Creating CDR for caller {cdr.CallerMSISDN} to receiver {cdr.ReceiverMSISDN}.");

        // Validate that CallerMSISDN and ReceiverMSISDN are different
        if (cdr.CallerMSISDN == cdr.ReceiverMSISDN)
        {
            _logger.LogWarning("Caller MSISDN and Receiver MSISDN are the same.");
            return BadRequest("Caller and receiver must be different.");
        }

        // Validate CallType is one of the allowed values
        if (!new[] { "local", "long-distance", "international" }.Contains(cdr.CallType.ToLower()))
        {
            return BadRequest("Invalid CallType. Allowed values are: local, long-distance, international.");
        }

        _context.CDRs.Add(cdr);
        await _context.SaveChangesAsync();

        _logger.LogInformation("CDR created successfully.");


        return CreatedAtAction(nameof(CreateCDR), new { id = cdr.Id }, new
        {
            CallerMSISDN = cdr.CallerMSISDN,
            ReceiverMSISDN = cdr.ReceiverMSISDN,
            Duration = cdr.Duration,
            Timestamp = cdr.Timestamp,
            CallType = cdr.CallType
        });
    }



    // PUT: api/CDR/{id}
    [HttpPut("Update CRD by ID")]
    public async Task<IActionResult> UpdateCDR(int id, CDR cdr)
    {
        if (id != cdr.Id)
        {
            return BadRequest();
        }

        _context.Entry(cdr).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.CDRs.Any(e => e.Id == id)) // if exists
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }




    // DELETE: api/CDR/{id}
    [HttpDelete(("Delete CRD by ID"))]
    public async Task<IActionResult> DeleteCDR(int id)
    {
        var cdr = await _context.CDRs.FindAsync(id);
        if (cdr == null)
        {
            return NotFound();
        }

        _context.CDRs.Remove(cdr);
        await _context.SaveChangesAsync();

        return NoContent();
    }



    [HttpGet("Calculate-charge by ID")]
    public async Task<ActionResult<decimal>> CalculateCharge(int id)
    {
        var cdr = await _context.CDRs.FindAsync(id);
        if (cdr == null)
        {
            return NotFound(); // = the 404 error
        }

        double ratePerMinute;
        switch (cdr.CallType.ToLower())
        {
            case "local":
                ratePerMinute = 0.05;
                break;
            case "long-distance":
                ratePerMinute = 0.10;
                break;
            case "international":
                ratePerMinute = 0.50;
                break;
            default:
                return BadRequest("Invalid call type"); // 400 err
        }

        // Calculate total cost based on duration in minutes
        var durationInMinutes = Math.Ceiling(cdr.Duration / 60.0); // to min ceil
        var charge = (double)durationInMinutes * ratePerMinute;

        return Ok(charge); // 200 workEd 
    }



    // total number of calls, total duration, and total charges for a specific user
        [HttpGet("summary by userID")]
    public async Task<ActionResult> GetUserSummary(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
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

        return Ok(new
        {
            TotalCalls = totalCalls,
            TotalDuration = totalDuration,
            TotalCharges = totalCharges
        });
    }


    /*  errored code
    [HttpGet("top-users")]
    public async Task<ActionResult> GetTopUsers()
    {
        var topUsers = await _context.Users
            .Select(user => new
            {
                UserId = user.Id,
                UserName = user.Name,
                TotalDuration = _context.CDRs
                    .Where(cdr => cdr.CallerMSISDN == user.MSISDN)
                    .Sum(cdr => cdr.Duration),
                TotalCharges = _context.CDRs
                    .Where(cdr => cdr.CallerMSISDN == user.MSISDN)
                    .Sum(cdr =>
                    {
                        decimal ratePerMinute = cdr.CallType.ToLower() switch
                        {
                            "local" => 0.05m,
                            "long-distance" => 0.10m,
                            "international" => 0.50m,
                            _ => 0m
                        };
                        return (decimal)Math.Ceiling(cdr.Duration / 60.0) * ratePerMinute;
                    })
            })
            .OrderByDescending(user => user.TotalDuration)
            .Take(5)
            .ToListAsync();

        return Ok(topUsers);
    }

    */


    //a list of top 5 users by call duration or charges
    [HttpGet("Get Top-users")]
    public async Task<ActionResult> GetTopUsers()
    {
        // get the CDR data we need
        var cdrData = await _context.CDRs
            .Select(cdr => new
            {
                cdr.CallerMSISDN,
                cdr.Duration,
                cdr.CallType
            })
            .ToListAsync();

        //  get the users
        var users = await _context.Users
            .Select(user => new
            {
                user.Id,
                user.Name,
                user.MSISDN
            })
            .ToListAsync();

        // calculations in memory
        var topUsers = users
            .Select(user =>
            {
                var userCDRs = cdrData.Where(cdr => cdr.CallerMSISDN == user.MSISDN);

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

                return new
                {
                    UserId = user.Id,
                    UserName = user.Name,
                    TotalDuration = totalDuration,
                    TotalCharges = totalCharges
                };
            })
            .OrderByDescending(user => user.TotalDuration)
            .Take(5)
            .ToList();

        return Ok(topUsers);
    }


}
