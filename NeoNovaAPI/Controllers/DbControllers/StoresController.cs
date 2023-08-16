using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class StoresController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public StoresController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        // GET: api/Stores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Store>>> GetStores()
        {
            string key = "stores";
            string cachedStores = _redisService.GetString(key);

            if (cachedStores != null)
            {
                return JsonConvert.DeserializeObject<List<Store>>(cachedStores);
            }

            var stores = await _context.Stores.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(stores), TimeSpan.FromHours(1));

            return stores;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Store>> GetStore(int id)
        {
            string key = $"store:{id}";
            string? cachedStore = _redisService.GetString(key); // Explicitly nullable

            if (cachedStore != null)
            {
                return JsonConvert.DeserializeObject<Store>(cachedStore);
            }

            var store = await _context.Stores.FindAsync(id);

            if (store == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(store), TimeSpan.FromHours(1));

            return store; // You've already checked for null
        }


        // PUT: api/Stores/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStore(int id, Store store)
        {
            if (id != store.Id)
            {
                return BadRequest();
            }

            _context.Entry(store).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StoreExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("stores"); // Invalidate the cache
            _redisService.DeleteKey($"store:{id}"); // Invalidate the specific cache entry

            return NoContent();
        }

        // POST: api/Stores
        [HttpPost]
        public async Task<ActionResult<Store>> PostStore(Store store)
        {
            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("stores"); // Invalidate the cache

            return CreatedAtAction("GetStore", new { id = store.Id }, store);
        }

        // DELETE: api/Stores/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStore(int id)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null)
            {
                return NotFound();
            }

            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("stores"); // Invalidate the cache
            _redisService.DeleteKey($"store:{id}"); // Invalidate the specific cache entry

            return NoContent();
        }

        private bool StoreExists(int id)
        {
            return (_context.Stores?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
