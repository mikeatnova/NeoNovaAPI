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
using NeoNovaAPI.Models.SecurityModels.Chat;

namespace NeoNovaAPI.Controllers.SecurityControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatLogController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public ChatLogController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        // GET: api/ChatLogs
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChatLog>>> GetChatLogs()
        {
            string key = "chatLogs";
            string cachedChatLogs = _redisService.GetString(key);

            if (cachedChatLogs != null)
            {
                return JsonConvert.DeserializeObject<List<ChatLog>>(cachedChatLogs);
            }

            var chatLogs = await _context.ChatLogs.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(chatLogs), TimeSpan.FromDays(7));

            return chatLogs;
        }

        // GET: api/ChatLogs/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ChatLog>> GetChatLog(int id)
        {
            string key = $"chatLog:{id}";
            string cachedChatLog = _redisService.GetString(key);

            if (cachedChatLog != null)
            {
                return JsonConvert.DeserializeObject<ChatLog>(cachedChatLog);
            }

            var chatLog = await _context.ChatLogs.FindAsync(id);

            if (chatLog == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(chatLog), TimeSpan.FromDays(7));

            return chatLog;
        }

        // PUT: api/ChatLogs/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChatLog(int id, ChatLog chatLog)
        {
            if (id != chatLog.ID)
            {
                return BadRequest();
            }

            _context.Entry(chatLog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChatLogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("chatLogs");
            _redisService.DeleteKey($"chatLog:{id}");

            return NoContent();
        }

        // POST: api/ChatLogs
        [Authorize(Policy = "SecurityTeam")]
        [HttpPost]
        public async Task<ActionResult<ChatLog>> PostChatLog(ChatLog chatLog)
        {
            _context.ChatLogs.Add(chatLog);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("chatLogs");

            return CreatedAtAction("GetChatLogs", new { id = chatLog.ID }, chatLog);
        }

        // DELETE: api/ChatLogs/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChatLog(int id)
        {
            var chatLog = await _context.ChatLogs.FindAsync(id);
            if (chatLog == null)
            {
                return NotFound();
            }

            _context.ChatLogs.Remove(chatLog);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("chatLogs");
            _redisService.DeleteKey($"chatLog:{id}");

            return NoContent();
        }

        private bool ChatLogExists(int id)
        {
            return (_context.ChatLogs?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
