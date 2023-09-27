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
using NeoNovaAPI.Models.SecurityModels.CameraManagement;

namespace NeoNovaAPI.Controllers.SecurityControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CameraLocationController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public CameraLocationController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        // GET: api/CameraLocations
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CameraLocation>>> GetCameraLocations()
        {
            string key = "cameraLocations";
            string cachedCameraLocations = _redisService.GetString(key);

            if (cachedCameraLocations != null)
            {
                return JsonConvert.DeserializeObject<List<CameraLocation>>(cachedCameraLocations);
            }

            var cameraLocations = await _context.CameraLocations.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(cameraLocations), TimeSpan.FromDays(7));

            return cameraLocations;
        }

        // GET: api/CameraLocations/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet("{id}")]
        public async Task<ActionResult<CameraLocation>> GetCameraLocation(int id)
        {
            string key = $"cameraLocation:{id}";
            string cachedCameraLocation = _redisService.GetString(key);

            if (cachedCameraLocation != null)
            {
                return JsonConvert.DeserializeObject<CameraLocation>(cachedCameraLocation);
            }

            var cameraLocation = await _context.CameraLocations.FindAsync(id);

            if (cameraLocation == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(cameraLocation), TimeSpan.FromDays(7));

            return cameraLocation;
        }

        // PUT: api/CameraLocations/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCameraLocation(int id, CameraLocation cameraLocation)
        {
            if (id != cameraLocation.ID)
            {
                return BadRequest();
            }

            _context.Entry(cameraLocation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CameraLocationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("cameraLocations");
            _redisService.DeleteKey($"cameraLocation:{id}");

            return NoContent();
        }

        // POST: api/CameraLocations
        [Authorize(Policy = "SecurityTeam")]
        [HttpPost]
        public async Task<ActionResult<CameraLocation>> PostCameraLocation(CameraLocation cameraLocation)
        {
            _context.CameraLocations.Add(cameraLocation);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("cameraLocations");

            return CreatedAtAction("GetCameraLocations", new { id = cameraLocation.ID }, cameraLocation);
        }

        // DELETE: api/CameraLocations/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCameraLocation(int id)
        {
            var cameraLocation = await _context.CameraLocations.FindAsync(id);
            if (cameraLocation == null)
            {
                return NotFound();
            }

            _context.CameraLocations.Remove(cameraLocation);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("cameraLocations");
            _redisService.DeleteKey($"cameraLocation:{id}");

            return NoContent();
        }

        private bool CameraLocationExists(int id)
        {
            return _context.CameraLocations.Any(e => e.ID == id);
        }
    }
}
