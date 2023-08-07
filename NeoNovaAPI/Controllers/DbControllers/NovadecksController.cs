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
    public class NovadecksController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;

        public NovadecksController(NeoNovaAPIDbContext context)
        {
            _context = context;
        }

        // GET: api/Novadecks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Novadeck>>> GetNovadecks()
        {
            if (_context.Novadecks == null)
            {
                return NotFound();
            }
            return await _context.Novadecks.ToListAsync();
        }

        // GET: api/Novadecks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Novadeck>> GetNovadeck(int id)
        {
            if (_context.Novadecks == null)
            {
                return NotFound();
            }
            var novadeck = await _context.Novadecks.FindAsync(id);

            if (novadeck == null)
            {
                return NotFound();
            }

            return novadeck;
        }

        // PUT: api/Novadecks/5
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

            return NoContent();
        }

        // POST: api/Novadecks
        [HttpPost]
        public async Task<ActionResult<Novadeck>> PostNovadeck(Novadeck novadeck)
        {
            if (_context.Novadecks == null)
            {
                return Problem("Entity set 'NeoNovaAPIDbContext.Novadecks'  is null.");
            }
            _context.Novadecks.Add(novadeck);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetNovadeck", new { id = novadeck.Id }, novadeck);
        }

        // DELETE: api/Novadecks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNovadeck(int id)
        {
            if (_context.Novadecks == null)
            {
                return NotFound();
            }
            var novadeck = await _context.Novadecks.FindAsync(id);
            if (novadeck == null)
            {
                return NotFound();
            }

            _context.Novadecks.Remove(novadeck);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NovadeckExists(int id)
        {
            return (_context.Novadecks?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
