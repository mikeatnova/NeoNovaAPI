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
    public class NovadecksController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public NovadecksController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        // GET: api/Novadecks
        [Authorize(Policy = "AllUsers")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Novadeck>>> GetNovadecks()
        {
            string key = "novadecks";
            string cachedNovadecks = _redisService.GetString(key);

            if (cachedNovadecks != null)
            {
                return JsonConvert.DeserializeObject<List<Novadeck>>(cachedNovadecks);
            }

            var novadecks = await _context.Novadecks.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(novadecks), TimeSpan.FromDays(7));

            return novadecks;
        }

        // GET: api/Novadecks/5
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Novadeck>> GetNovadeck(int id)
        {
            string key = $"novadeck:{id}";
            string cachedNovadeck = _redisService.GetString(key);

            if (cachedNovadeck != null)
            {
                return JsonConvert.DeserializeObject<Novadeck>(cachedNovadeck);
            }

            var novadeck = await _context.Novadecks.FindAsync(id);

            if (novadeck == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(novadeck), TimeSpan.FromDays(7));

            return novadeck;
        }

        // PUT: api/Novadecks/5
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNovadeck(int id, Novadeck novadeck)
        {
            if (id != novadeck.Id)
            {
                return BadRequest();
            }

            _context.Entry(novadeck).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NovadeckExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("novadecks"); // Invalidate the cache
            _redisService.DeleteKey($"novadeck:{id}"); // Invalidate the specific cache entry

            return NoContent();
        }

        // POST: api/Novadecks
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<ActionResult<Novadeck>> PostNovadeck(Novadeck novadeck)
        {
            _context.Novadecks.Add(novadeck);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("novadecks"); // Invalidate the cache

            return CreatedAtAction("GetNovadeck", new { id = novadeck.Id }, novadeck);
        }

        // DELETE: api/Novadecks/5
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNovadeck(int id)
        {
            var novadeck = await _context.Novadecks.FindAsync(id);
            if (novadeck == null)
            {
                return NotFound();
            }

            _context.Novadecks.Remove(novadeck);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("novadecks"); // Invalidate the cache
            _redisService.DeleteKey($"novadeck:{id}"); // Invalidate the specific cache entry

            return NoContent();
        }

        private bool NovadeckExists(int id)
        {
            return (_context.Novadecks?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
