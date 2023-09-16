using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    public class CameraHistoryController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public CameraHistoryController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        [Authorize(Policy = "SecurityTeam")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CameraHistory>>> GetCameraHistories()
        {
            string key = "cameraHistories";
            string cachedHistories = _redisService.GetString(key);

            if (cachedHistories != null)
            {
                return JsonConvert.DeserializeObject<List<CameraHistory>>(cachedHistories);
            }

            var cameraHistories = await _context.CameraHistories.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(cameraHistories), TimeSpan.FromDays(7));

            return cameraHistories;
        }

        [Authorize(Policy = "SecurityTeam")]
        [HttpGet("{id}")]
        public async Task<ActionResult<CameraHistory>> GetCameraHistory(int id)
        {
            string key = $"cameraHistory:{id}";
            string cachedHistory = _redisService.GetString(key);

            if (cachedHistory != null)
            {
                return JsonConvert.DeserializeObject<CameraHistory>(cachedHistory);
            }

            var cameraHistory = await _context.CameraHistories.FindAsync(id);

            if (cameraHistory == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(cameraHistory), TimeSpan.FromDays(7));

            return cameraHistory;
        }

        [Authorize(Policy = "SecurityTeam")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCameraHistory(int id, CameraHistory cameraHistory)
        {
            if (id != cameraHistory.ID)
            {
                return BadRequest();
            }

            _context.Entry(cameraHistory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CameraHistoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("cameraHistories");
            _redisService.DeleteKey($"cameraHistory:{id}");

            return NoContent();
        }

        [Authorize(Policy = "SecurityTeam")]
        [HttpPost]
        public async Task<ActionResult<CameraHistory>> PostCameraHistory(CameraHistory cameraHistory)
        {
            _context.CameraHistories.Add(cameraHistory);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("cameraHistories");

            return CreatedAtAction("GetCameraHistories", new { id = cameraHistory.ID }, cameraHistory);
        }

        [Authorize(Policy = "SecurityTeam")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCameraHistory(int id)
        {
            var cameraHistory = await _context.CameraHistories.FindAsync(id);
            if (cameraHistory == null)
            {
                return NotFound();
            }

            _context.CameraHistories.Remove(cameraHistory);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("cameraHistories");
            _redisService.DeleteKey($"cameraHistory:{id}");

            return NoContent();
        }

        private bool CameraHistoryExists(int id)
        {
            return (_context.CameraHistories?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}

