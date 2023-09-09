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
    public class FaqsController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public FaqsController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }


        // GET: api/Faqs
        [Authorize(Policy = "AllUsers")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Faq>>> GetFaqs()
        {
            string key = "faqs";
            string cachedFaqs = _redisService.GetString(key);

            if (cachedFaqs != null)
            {

                return JsonConvert.DeserializeObject<List<Faq>>(cachedFaqs);
            }

            var faqs = await _context.Faqs.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(faqs), TimeSpan.FromDays(7));

            return faqs;
        }


        // GET: api/Faqs/5
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Faq>> GetFaq(int id)
        {
            string key = $"faq:{id}";
            string cachedFaq = _redisService.GetString(key);

            if (cachedFaq != null)
            {
                return JsonConvert.DeserializeObject<Faq>(cachedFaq);
            }

            var faq = await _context.Faqs.FindAsync(id);

            if (faq == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(faq), TimeSpan.FromDays(7));

            return faq;
        }

        // PUT: api/Faqs/5
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFaq(int id, Faq faq)
        {
            if (id != faq.Id)
            {
                return BadRequest();
            }

            _context.Entry(faq).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FaqExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("faqs"); // Invalidate the cache
            _redisService.DeleteKey($"faq:{id}"); // Invalidate the specific cache entry


            return NoContent();
        }

        // POST: api/Faqs
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<ActionResult<Faq>> PostFaq(Faq faq)
        {
            if (_context.Faqs == null)
            {
                return Problem("Entity set 'NeoNovaAPIDbContext.Faqs'  is null.");
            }
            _context.Faqs.Add(faq);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("faqs"); // Invalidate the cache

            return CreatedAtAction("GetFaq", new { id = faq.Id }, faq);
        }

        // DELETE: api/Faqs/5
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFaq(int id)
        {
            if (_context.Faqs == null)
            {
                return NotFound();
            }
            var faq = await _context.Faqs.FindAsync(id);
            if (faq == null)
            {
                return NotFound();
            }

            _context.Faqs.Remove(faq);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("faqs"); // Invalidate the cache
            _redisService.DeleteKey($"faq:{id}"); // Invalidate the specific cache entry


            return NoContent();
        }

        private bool FaqExists(int id)
        {
            return (_context.Faqs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
