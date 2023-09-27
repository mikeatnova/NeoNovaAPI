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
using NeoNovaAPI.Models.SecurityModels.Archiving;
using NeoNovaAPI.Models.SecurityModels.CameraManagement;

namespace NeoNovaAPI.Controllers.SecurityControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CameraStatusController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public CameraStatusController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        // GET: api/CameraStatuses
        //[AllowAnonymous]
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CameraStatus>>> GetCameraStatuses()
        {
            string key = "cameras";
            string cachedCameraStatuses = _redisService.GetString(key);

            if (cachedCameraStatuses != null)
            {

                return JsonConvert.DeserializeObject<List<CameraStatus>>(cachedCameraStatuses);
            }

            var cameraStatuses = await _context.CameraStatuses.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(cameraStatuses), TimeSpan.FromDays(7));

            return cameraStatuses;
        }

        // GET: api/Cameras/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet("{id}")]
        public async Task<ActionResult<CameraStatus>> GetCameraStatus(int id)
        {
            string key = $"cameraStatus:{id}";
            string cachedCameraStatus = _redisService.GetString(key);

            if (cachedCameraStatus != null)
            {
                return JsonConvert.DeserializeObject<CameraStatus>(cachedCameraStatus);
            }

            var cameraStatus = await _context.CameraStatuses.FindAsync(id);

            if (cameraStatus == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(cameraStatus), TimeSpan.FromDays(7));

            return cameraStatus;
        }

        // PUT: api/Cameras/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCamera(int id, CameraStatus cameraStatus)
        {
            if (id != cameraStatus.ID)
            {
                return BadRequest();
            }

            _context.Entry(cameraStatus).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CameraStatusExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("cameraStatuses"); // Invalidate the cache
            _redisService.DeleteKey($"cameraStatus:{id}"); // Invalidate the specific cache entry

            return NoContent();
        }

        // POST: api/Cameras
        [Authorize(Policy = "SecurityTeam")]
        [HttpPost]
        public async Task<ActionResult<Camera>> PostArchive(CameraStatus cameraStatus)
        {
            _context.CameraStatuses.Add(cameraStatus);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("cameraStatuses"); // Invalidate the cache

            return CreatedAtAction("GetCameraStatuses", new { id = cameraStatus.ID }, cameraStatus);
        }


        // DELETE: api/Cameras/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCameraStatus(int id)
        {
            var cameraStatus = await _context.CameraStatuses.FindAsync(id);
            if (cameraStatus == null)
            {
                return NotFound();
            }

            _context.CameraStatuses.Remove(cameraStatus);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("cameraStatuses"); // Invalidate the cache
            _redisService.DeleteKey($"cameraStatus:{id}"); // Invalidate the specific cache entry

            return NoContent();
        }

        private bool CameraStatusExists(int id)
        {
            return (_context.CameraStatuses?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
