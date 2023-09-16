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
using NeoNovaAPI.Models.SecurityModels.TourManagement;

namespace NeoNovaAPI.Controllers.SecurityControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public TourController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        // GET: api/Tours
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tour>>> GetTours()
        {
            string key = "tours";
            string cachedTours = _redisService.GetString(key);

            if (cachedTours != null)
            {
                return JsonConvert.DeserializeObject<List<Tour>>(cachedTours);
            }

            var tours = await _context.Tours.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(tours), TimeSpan.FromDays(7));

            return tours;
        }

        // GET: api/Tours/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Tour>> GetTour(int id)
        {
            string key = $"tour:{id}";
            string cachedTour = _redisService.GetString(key);

            if (cachedTour != null)
            {
                return JsonConvert.DeserializeObject<Tour>(cachedTour);
            }

            var tour = await _context.Tours.FindAsync(id);

            if (tour == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(tour), TimeSpan.FromDays(7));

            return tour;
        }

        // PUT: api/Tours/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTour(int id, Tour tour)
        {
            if (id != tour.ID)
            {
                return BadRequest();
            }

            _context.Entry(tour).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TourExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("tours");
            _redisService.DeleteKey($"tour:{id}");

            return NoContent();
        }

        // POST: api/Tours
        [Authorize(Policy = "SecurityTeam")]
        [HttpPost]
        public async Task<ActionResult<Tour>> PostTour(Tour tour)
        {
            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("tours");

            return CreatedAtAction("GetTours", new { id = tour.ID }, tour);
        }

        // DELETE: api/Tours/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTour(int id)
        {
            var tour = await _context.Tours.FindAsync(id);
            if (tour == null)
            {
                return NotFound();
            }

            _context.Tours.Remove(tour);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("tours");
            _redisService.DeleteKey($"tour:{id}");

            return NoContent();
        }

        private bool TourExists(int id)
        {
            return (_context.Tours?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
