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
    public class StoreHoursController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public StoreHoursController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        // GET: api/StoreHours
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StoreHour>>> GetStoreHours()
        {
            string key = "storeHours";
            string cachedStoreHours = _redisService.GetString(key);

            if (cachedStoreHours != null)
            {
                return JsonConvert.DeserializeObject<List<StoreHour>>(cachedStoreHours);
            }

            var storeHours = await _context.StoreHours.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(storeHours), TimeSpan.FromHours(1));

            return storeHours;
        }

        // GET: api/StoreHours/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StoreHour>> GetStoreHour(int id)
        {
            string key = $"storeHour:{id}";
            string cachedStoreHour = _redisService.GetString(key);

            if (cachedStoreHour != null)
            {
                return JsonConvert.DeserializeObject<StoreHour>(cachedStoreHour);
            }

            var storeHour = await _context.StoreHours.FindAsync(id);

            if (storeHour == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(storeHour), TimeSpan.FromHours(1));

            return storeHour;
        }

        // PUT: api/StoreHours/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStoreHour(int id, StoreHour storeHour)
        {
            if (id != storeHour.Id)
            {
                return BadRequest();
            }

            _context.Entry(storeHour).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StoreHourExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("storeHours"); // Invalidate the cache
            _redisService.DeleteKey($"storeHour:{id}"); // Invalidate the specific cache entry

            return NoContent();
        }

        // POST: api/StoreHours
        [HttpPost]
        public async Task<ActionResult<StoreHour>> PostStoreHour(StoreHour storeHour)
        {
            _context.StoreHours.Add(storeHour);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("storeHours"); // Invalidate the cache

            return CreatedAtAction("GetStoreHour", new { id = storeHour.Id }, storeHour);
        }

        // DELETE: api/StoreHours/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStoreHour(int id)
        {
            var storeHour = await _context.StoreHours.FindAsync(id);
            if (storeHour == null)
            {
                return NotFound();
            }

            _context.StoreHours.Remove(storeHour);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("storeHours"); // Invalidate the cache
            _redisService.DeleteKey($"storeHour:{id}"); // Invalidate the specific cache entry

            return NoContent();
        }

        private bool StoreHourExists(int id)
        {
            return (_context.StoreHours?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
