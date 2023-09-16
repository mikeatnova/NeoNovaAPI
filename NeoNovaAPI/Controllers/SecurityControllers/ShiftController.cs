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
    public class ShiftController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public ShiftController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        // GET: api/Shifts
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shift>>> GetShifts()
        {
            string key = "shifts";
            string cachedShifts = _redisService.GetString(key);

            if (cachedShifts != null)
            {
                return JsonConvert.DeserializeObject<List<Shift>>(cachedShifts);
            }

            var shifts = await _context.Shifts.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(shifts), TimeSpan.FromDays(7));

            return shifts;
        }

        // GET: api/Shifts/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Shift>> GetShift(int id)
        {
            string key = $"shift:{id}";
            string cachedShift = _redisService.GetString(key);

            if (cachedShift != null)
            {
                return JsonConvert.DeserializeObject<Shift>(cachedShift);
            }

            var shift = await _context.Shifts.FindAsync(id);

            if (shift == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(shift), TimeSpan.FromDays(7));

            return shift;
        }

        // PUT: api/Shifts/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutShift(int id, Shift shift)
        {
            if (id != shift.ID)
            {
                return BadRequest();
            }

            _context.Entry(shift).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShiftExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("shifts");
            _redisService.DeleteKey($"shift:{id}");

            return NoContent();
        }

        // POST: api/Shifts
        [Authorize(Policy = "SecurityTeam")]
        [HttpPost]
        public async Task<ActionResult<Shift>> PostShift(Shift shift)
        {
            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("shifts");

            return CreatedAtAction("GetShifts", new { id = shift.ID }, shift);
        }

        // DELETE: api/Shifts/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShift(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
            {
                return NotFound();
            }

            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("shifts");
            _redisService.DeleteKey($"shift:{id}");

            return NoContent();
        }

        private bool ShiftExists(int id)
        {
            return (_context.Shifts?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
