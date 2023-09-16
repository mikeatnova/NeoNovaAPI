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

namespace NeoNovaAPI.Controllers.SecurityControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArchiveController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public ArchiveController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        // GET: api/Archives
        //[AllowAnonymous]
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Archive>>> GetArchives()
        {
            string key = "archives";
            string cachedArchives = _redisService.GetString(key);

            if (cachedArchives != null)
            {

                return JsonConvert.DeserializeObject<List<Archive>>(cachedArchives);
            }

            var archives = await _context.Archives.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(archives), TimeSpan.FromDays(7));

            return archives;
        }

        // GET: api/Archives/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Archive>> GetArchive(int id)
        {
            string key = $"archive:{id}";
            string cachedArchive = _redisService.GetString(key);

            if (cachedArchive != null)
            {
                return JsonConvert.DeserializeObject<Archive>(cachedArchive);
            }

            var archive = await _context.Archives.FindAsync(id);

            if (archive == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(archive), TimeSpan.FromDays(7));

            return archive;
        }

        // PUT: api/Archives/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArchive(int id, Archive archive)
        {
            if (id != archive.ID)
            {
                return BadRequest();
            }

            _context.Entry(archive).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArchiveExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("archives"); // Invalidate the cache
            _redisService.DeleteKey($"archive:{id}"); // Invalidate the specific cache entry

            return NoContent();
        }

        // POST: api/Archives
        [Authorize(Policy = "SecurityTeam")]
        [HttpPost]
        public async Task<ActionResult<Archive>> PostArchive(Archive archive)
        {
            _context.Archives.Add(archive);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("archives"); // Invalidate the cache

            return CreatedAtAction("GetArchives", new { id = archive.ID }, archive);
        }


        // DELETE: api/Archives/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArchive(int id)
        {
            var archive = await _context.Archives.FindAsync(id);
            if (archive == null)
            {
                return NotFound();
            }

            _context.Archives.Remove(archive);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("archives"); // Invalidate the cache
            _redisService.DeleteKey($"archive:{id}"); // Invalidate the specific cache entry

            return NoContent();
        }

        private bool ArchiveExists(int id)
        {
            return (_context.Archives?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
