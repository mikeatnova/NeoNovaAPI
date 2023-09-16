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
using NeoNovaAPI.Models.SecurityModels.ShiftManagement;

namespace NeoNovaAPI.Controllers.SecurityControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftNoteController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public ShiftNoteController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        // GET: api/ShiftNotes
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShiftNote>>> GetShiftNotes()
        {
            string key = "shiftNotes";
            string cachedShiftNotes = _redisService.GetString(key);

            if (cachedShiftNotes != null)
            {
                return JsonConvert.DeserializeObject<List<ShiftNote>>(cachedShiftNotes);
            }

            var shiftNotes = await _context.ShiftNotes.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(shiftNotes), TimeSpan.FromDays(7));

            return shiftNotes;
        }

        // GET: api/ShiftNotes/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftNote>> GetShiftNote(int id)
        {
            string key = $"shiftNote:{id}";
            string cachedShiftNote = _redisService.GetString(key);

            if (cachedShiftNote != null)
            {
                return JsonConvert.DeserializeObject<ShiftNote>(cachedShiftNote);
            }

            var shiftNote = await _context.ShiftNotes.FindAsync(id);

            if (shiftNote == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(shiftNote), TimeSpan.FromDays(7));

            return shiftNote;
        }

        // PUT: api/ShiftNotes/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutShiftNote(int id, ShiftNote shiftNote)
        {
            if (id != shiftNote.ID)
            {
                return BadRequest();
            }

            _context.Entry(shiftNote).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShiftNoteExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("shiftNotes");
            _redisService.DeleteKey($"shiftNote:{id}");

            return NoContent();
        }

        // POST: api/ShiftNotes
        [Authorize(Policy = "SecurityTeam")]
        [HttpPost]
        public async Task<ActionResult<ShiftNote>> PostShiftNote(ShiftNote shiftNote)
        {
            _context.ShiftNotes.Add(shiftNote);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("shiftNotes");

            return CreatedAtAction("GetShiftNotes", new { id = shiftNote.ID }, shiftNote);
        }

        // DELETE: api/ShiftNotes/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShiftNote(int id)
        {
            var shiftNote = await _context.ShiftNotes.FindAsync(id);
            if (shiftNote == null)
            {
                return NotFound();
            }

            _context.ShiftNotes.Remove(shiftNote);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("shiftNotes");
            _redisService.DeleteKey($"shiftNote:{id}");

            return NoContent();
        }

        private bool ShiftNoteExists(int id)
        {
            return (_context.ShiftNotes?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
