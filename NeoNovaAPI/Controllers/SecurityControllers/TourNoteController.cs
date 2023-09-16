using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NeoNovaAPI.Data;
using NeoNovaAPI.Models.SecurityModels.TourManagement;
using NeoNovaAPI.Services;
using Newtonsoft.Json;

namespace NeoNovaAPI.Controllers.SecurityControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourNoteController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public TourNoteController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        // GET: api/TourNotes
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TourNote>>> GetTourNotes()
        {
            string key = "tourNotes";
            string cachedTourNotes = _redisService.GetString(key);

            if (cachedTourNotes != null)
            {
                return JsonConvert.DeserializeObject<List<TourNote>>(cachedTourNotes);
            }

            var tourNotes = await _context.TourNotes.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(tourNotes), TimeSpan.FromDays(7));

            return tourNotes;
        }

        // GET: api/TourNotes/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet("{id}")]
        public async Task<ActionResult<TourNote>> GetTourNote(int id)
        {
            string key = $"tourNote:{id}";
            string cachedTourNote = _redisService.GetString(key);

            if (cachedTourNote != null)
            {
                return JsonConvert.DeserializeObject<TourNote>(cachedTourNote);
            }

            var tourNote = await _context.TourNotes.FindAsync(id);

            if (tourNote == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(tourNote), TimeSpan.FromDays(7));

            return tourNote;
        }

        // PUT: api/TourNotes/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTourNote(int id, TourNote tourNote)
        {
            if (id != tourNote.ID)
            {
                return BadRequest();
            }

            _context.Entry(tourNote).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TourNoteExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("tourNotes");
            _redisService.DeleteKey($"tourNote:{id}");

            return NoContent();
        }

        // POST: api/TourNotes
        [Authorize(Policy = "SecurityTeam")]
        [HttpPost]
        public async Task<ActionResult<TourNote>> PostTourNote(TourNote tourNote)
        {
            _context.TourNotes.Add(tourNote);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("tourNotes");

            return CreatedAtAction("GetTourNotes", new { id = tourNote.ID }, tourNote);
        }

        // DELETE: api/TourNotes/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTourNote(int id)
        {
            var tourNote = await _context.TourNotes.FindAsync(id);
            if (tourNote == null)
            {
                return NotFound();
            }

            _context.TourNotes.Remove(tourNote);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("tourNotes");
            _redisService.DeleteKey($"tourNote:{id}");

            return NoContent();
        }

        private bool TourNoteExists(int id)
        {
            return (_context.TourNotes?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
