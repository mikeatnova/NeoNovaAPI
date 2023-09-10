using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoNovaAPI.Data;
using NeoNovaAPI.Models.WholesaleModels;
using NeoNovaAPI.Services;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NeoNovaAPI.Controllers.DbControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WholesaleBugMessagesController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public WholesaleBugMessagesController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        // GET: api/WholesaleBugMessages
        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WholesaleBugMessage>>> GetWholesaleBugMessages()
        {
            string key = "wholesaleBugMessages";
            string cachedWholesaleBugMessages = _redisService.GetString(key);

            if (cachedWholesaleBugMessages != null)
            {
                return JsonConvert.DeserializeObject<List<WholesaleBugMessage>>(cachedWholesaleBugMessages);
            }

            var wholesaleBugMessages = await _context.WholesaleBugMessages.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(wholesaleBugMessages), TimeSpan.FromHours(1));

            return wholesaleBugMessages;
        }

        // GET: api/WholesaleBugMessages/5
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("{id}")]
        public async Task<ActionResult<WholesaleBugMessage>> GetWholesaleBugMessage(int id)
        {
            string key = $"wholesaleBugMessage:{id}";
            string cachedWholesaleBugMessage = _redisService.GetString(key);

            if (cachedWholesaleBugMessage != null)
            {
                return JsonConvert.DeserializeObject<WholesaleBugMessage>(cachedWholesaleBugMessage);
            }

            var wholesaleBugMessage = await _context.WholesaleBugMessages.FindAsync(id);

            if (wholesaleBugMessage == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(wholesaleBugMessage), TimeSpan.FromHours(1));

            return wholesaleBugMessage;
        }

        // PUT: api/WholesaleBugMessages/5
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWholesaleBugMessage(int id, WholesaleBugMessage wholesaleBugMessage)
        {
            if (id != wholesaleBugMessage.Id)
            {
                return BadRequest();
            }

            _context.Entry(wholesaleBugMessage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WholesaleBugMessageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("wholesaleBugMessages"); // Invalidate the cache
            _redisService.DeleteKey($"wholesaleBugMessage:{id}"); // Invalidate the specific cache entry

            return NoContent();
        }

        // POST: api/WholesaleBugMessages
        //[Authorize(Policy = "AdminOnly")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<WholesaleBugMessage>> PostWholesaleBugMessage(WholesaleBugMessage wholesaleBugMessage)
        {
            try
            {
                if (_context.WholesaleBugMessages == null)
                {
                    return Problem("Entity set 'NeoNovaAPIDbContext.WholesaleBugMessages' is null.");
                }

                _context.WholesaleBugMessages.Add(wholesaleBugMessage);
                await _context.SaveChangesAsync();

                _redisService.DeleteKey("wholesaleBugMessages"); // Invalidate the cache

                return CreatedAtAction("GetWholesaleBugMessage", new { id = wholesaleBugMessage.Id }, wholesaleBugMessage);
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database-specific errors
                return StatusCode(500, $"Database Error: {dbEx.Message}");
            }
            catch (RedisException redisEx)
            {
                // Handle Redis-specific errors
                return StatusCode(500, $"Redis Error: {redisEx.Message}");
            }
            catch (Exception ex)
            {
                // General error handling
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }


        // DELETE: api/WholesaleBugMessages/5
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWholesaleBugMessage(int id)
        {
            if (_context.WholesaleBugMessages == null)
            {
                return NotFound();
            }
            var wholesaleBugMessage = await _context.WholesaleBugMessages.FindAsync(id);
            if (wholesaleBugMessage == null)
            {
                return NotFound();
            }

            _context.WholesaleBugMessages.Remove(wholesaleBugMessage);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("wholesaleBugMessages"); // Invalidate the cache
            _redisService.DeleteKey($"wholesaleBugMessage:{id}"); // Invalidate the specific cache entry

            return NoContent();
        }

        private bool WholesaleBugMessageExists(int id)
        {
            return (_context.WholesaleBugMessages?.Any(e => e.Id == id)).GetValueOrDefault();
        }

    }
}
