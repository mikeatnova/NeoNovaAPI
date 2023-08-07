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
    public class GeofencesController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;

        public GeofencesController(NeoNovaAPIDbContext context)
        {
            _context = context;
        }

        // GET: api/Geofences
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Geofence>>> GetGeofences()
        {
            if (_context.Geofences == null)
            {
                return NotFound();
            }
            return await _context.Geofences.ToListAsync();
        }

        // GET: api/Geofences/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Geofence>> GetGeofence(int id)
        {
            if (_context.Geofences == null)
            {
                return NotFound();
            }
            var geofence = await _context.Geofences.FindAsync(id);

            if (geofence == null)
            {
                return NotFound();
            }

            return geofence;
        }

        // PUT: api/Geofences/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGeofence(int id, Geofence geofence)
        {
            if (id != geofence.Id)
            {
                return BadRequest();
            }

            _context.Entry(geofence).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GeofenceExists(id))
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

        // POST: api/Geofences
        [HttpPost]
        public async Task<ActionResult<Geofence>> PostGeofence(Geofence geofence)
        {
            if (_context.Geofences == null)
            {
                return Problem("Entity set 'NeoNovaAPIDbContext.Geofences'  is null.");
            }
            _context.Geofences.Add(geofence);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGeofence", new { id = geofence.Id }, geofence);
        }

        // DELETE: api/Geofences/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGeofence(int id)
        {
            if (_context.Geofences == null)
            {
                return NotFound();
            }
            var geofence = await _context.Geofences.FindAsync(id);
            if (geofence == null)
            {
                return NotFound();
            }

            _context.Geofences.Remove(geofence);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GeofenceExists(int id)
        {
            return (_context.Geofences?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
