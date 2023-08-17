using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoNovaAPI.Data;
using NeoNovaAPI.Models.DbModels;
using NeoNovaAPI.Services;
using Newtonsoft.Json;

namespace NeoNovaAPI.Controllers.DbControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeofencesController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;

        private readonly RedisService _redisService;


        public GeofencesController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;

        }

        // GET: api/Geofences
        [Authorize(Policy = "AllUsers")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Geofence>>> GetGeofences()
        {
            string key = "geofences";
            string cachedGeofences = _redisService.GetString(key);

            if (cachedGeofences != null)
            {
                return JsonConvert.DeserializeObject<List<Geofence>>(cachedGeofences);
            }

            var geofences = await _context.Geofences.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(geofences), TimeSpan.FromHours(1));

            return geofences;
        }

        // GET: api/Geofences/5
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Geofence>> GetGeofence(int id)
        {
            string key = $"geofence:{id}";
            string cachedGeofence = _redisService.GetString(key);

            if (cachedGeofence != null)
            {
                return JsonConvert.DeserializeObject<Geofence>(cachedGeofence);
            }

            var geofence = await _context.Geofences.FindAsync(id);

            if (geofence == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(geofence), TimeSpan.FromHours(1));

            return geofence;
        }

        // PUT: api/Geofences/5
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGeofence(int id, Geofence geofence)
        {
            if (id != geofence.Id)
            {
                return BadRequest();
            }

            _context.Entry(geofence).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GeofenceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("geofences"); // Invalidate the cache
            _redisService.DeleteKey($"geofence:{id}"); // Invalidate the specific cache entry

            return NoContent();
        }

        // POST: api/Geofences
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<ActionResult<Geofence>> PostGeofence(Geofence geofence)
        {
            _context.Geofences.Add(geofence);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("geofences"); // Invalidate the cache

            return CreatedAtAction("GetGeofence", new { id = geofence.Id }, geofence);
        }


        // DELETE: api/Geofences/5
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGeofence(int id)
        {
            var geofence = await _context.Geofences.FindAsync(id);
            if (geofence == null)
            {
                return NotFound();
            }

            _context.Geofences.Remove(geofence);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("geofences"); // Invalidate the cache
            _redisService.DeleteKey($"geofence:{id}"); // Invalidate the specific cache entry

            return NoContent();
        }

        private bool GeofenceExists(int id)
        {
            return (_context.Geofences?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
