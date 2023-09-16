using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NeoNovaAPI.Data;
using NeoNovaAPI.Models.DbModels;
using NeoNovaAPI.Services;
using Newtonsoft.Json;
using NeoNovaAPI.Models.SecurityModels.CameraManagment;

namespace NeoNovaAPI.Controllers.SecurityControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public LocationController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        // GET: api/Locations
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
        {
            string key = "locations";
            string cachedLocations = _redisService.GetString(key);

            if (cachedLocations != null)
            {
                return JsonConvert.DeserializeObject<List<Location>>(cachedLocations);
            }

            var locations = await _context.Locations.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(locations), TimeSpan.FromDays(7));

            return locations;
        }

        // GET: api/Locations/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Location>> GetLocation(int id)
        {
            string key = $"location:{id}";
            string cachedLocation = _redisService.GetString(key);

            if (cachedLocation != null)
            {
                return JsonConvert.DeserializeObject<Location>(cachedLocation);
            }

            var location = await _context.Locations.FindAsync(id);

            if (location == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(location), TimeSpan.FromDays(7));

            return location;
        }

        // PUT: api/Locations/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLocation(int id, Location location)
        {
            if (id != location.ID)
            {
                return BadRequest();
            }

            _context.Entry(location).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("locations");
            _redisService.DeleteKey($"location:{id}");

            return NoContent();
        }

        // POST: api/Locations
        [Authorize(Policy = "SecurityTeam")]
        [HttpPost]
        public async Task<ActionResult<Location>> PostLocation(Location location)
        {
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("locations");

            return CreatedAtAction("GetLocations", new { id = location.ID }, location);
        }

        // DELETE: api/Locations/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("locations");
            _redisService.DeleteKey($"location:{id}");

            return NoContent();
        }

        private bool LocationExists(int id)
        {
            return (_context.Locations?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
