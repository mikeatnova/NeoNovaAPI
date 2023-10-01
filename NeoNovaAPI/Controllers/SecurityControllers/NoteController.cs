using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NeoNovaAPI.Data;
using NeoNovaAPI.Models.SecurityModels.Reporting;
using Newtonsoft.Json;
using NeoNovaAPI.Services;
using Microsoft.AspNetCore.JsonPatch;

namespace NeoNovaAPI.Controllers.SecurityControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoteController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public NoteController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        [Authorize(Policy = "SecurityTeam")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotes()
        {
            string key = "notes";
            string cachedNotes = _redisService.GetString(key);

            if (cachedNotes != null)
            {
                return JsonConvert.DeserializeObject<List<Note>>(cachedNotes);
            }

            var notes = await _context.Notes.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(notes), TimeSpan.FromMinutes(1));

            return notes;
        }

        [Authorize(Policy = "SecurityTeam")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Note>> GetNote(int id)
        {
            string key = $"note:{id}";
            string cachedNote = _redisService.GetString(key);

            if (cachedNote != null)
            {
                return JsonConvert.DeserializeObject<Note>(cachedNote);
            }

            var note = await _context.Notes.FindAsync(id);

            if (note == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(note), TimeSpan.FromMinutes(1));

            return note;
        }

        [Authorize(Policy = "SecurityTeam")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNote(int id, Note note)
        {
            if (id != note.ID)
            {
                return BadRequest();
            }

            _context.Entry(note).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NoteExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("notes");
            _redisService.DeleteKey($"note:{id}");

            return NoContent();
        }

        [Authorize(Policy = "SecurityTeam")]
        [HttpPost]
        public async Task<ActionResult<Note>> PostNote(Note note)
        {
            try
            {
                DebugUtility.DebugAttributes(note);
                _context.Notes.Add(note);
                await _context.SaveChangesAsync();
                _redisService.DeleteKey("notes");
                return CreatedAtAction("GetNotes", new { id = note.ID }, note);
            }
            catch (Exception e)
            {
                // Log the full exception details
                DebugUtility.DebugLine($"Error: {e.Message}");
                DebugUtility.DebugLine($"Inner Exception: {e.InnerException?.Message ?? "N/A"}");
                DebugUtility.DebugLine($"Stack Trace: {e.StackTrace}");

                // Also log the exception details to the console
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine($"Inner Exception: {e.InnerException?.Message ?? "N/A"}");
                Console.WriteLine($"Stack Trace: {e.StackTrace}");

                return BadRequest(new { error = "An error occurred while saving the entity changes. See the logs for details." });
            }
        }


        [Authorize(Policy = "SecurityTeam")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchNote(int id, [FromBody] JsonPatchDocument<Note> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var existingNote = await _context.Notes.FindAsync(id);

            if (existingNote == null)
            {
                return NotFound();
            }

            patchDoc.ApplyTo(existingNote, error =>
            {
                ModelState.AddModelError(
                    key: error?.AffectedObject?.ToString() ?? string.Empty,
                    errorMessage: error?.ErrorMessage ?? string.Empty
                );
            });

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(existingNote).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _redisService.DeleteKey($"note:{id}");

            return NoContent();
        }


        [Authorize(Policy = "SecurityTeam")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound();
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("notes");
            _redisService.DeleteKey($"note:{id}");

            return NoContent();
        }

        private bool NoteExists(int id)
        {
            return (_context.Notes?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
