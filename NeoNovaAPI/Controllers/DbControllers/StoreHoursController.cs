using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoNovaAPI.Data;
using NeoNovaAPI.Models.DbModels;

namespace NeoNovaAPI.Controllers.DbControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreHoursController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;

        public StoreHoursController(NeoNovaAPIDbContext context)
        {
            _context = context;
        }

        // GET: api/StoreHours
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StoreHour>>> GetStoreHours()
        {
            if (_context.StoreHours == null)
            {
                return NotFound();
            }
            return await _context.StoreHours.ToListAsync();
        }

        // GET: api/StoreHours/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StoreHour>> GetStoreHour(int id)
        {
            if (_context.StoreHours == null)
            {
                return NotFound();
            }
            var storeHour = await _context.StoreHours.FindAsync(id);

            if (storeHour == null)
            {
                return NotFound();
            }

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

            return NoContent();
        }

        // POST: api/StoreHours
        [HttpPost]
        public async Task<ActionResult<StoreHour>> PostStoreHour(StoreHour storeHour)
        {
            if (_context.StoreHours == null)
            {
                return Problem("Entity set 'NeoNovaAPIDbContext.StoreHours'  is null.");
            }
            _context.StoreHours.Add(storeHour);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStoreHour", new { id = storeHour.Id }, storeHour);
        }

        // DELETE: api/StoreHours/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStoreHour(int id)
        {
            if (_context.StoreHours == null)
            {
                return NotFound();
            }
            var storeHour = await _context.StoreHours.FindAsync(id);
            if (storeHour == null)
            {
                return NotFound();
            }

            _context.StoreHours.Remove(storeHour);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StoreHourExists(int id)
        {
            return (_context.StoreHours?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
