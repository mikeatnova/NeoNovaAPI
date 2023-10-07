using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoNovaAPI.Data;
using NeoNovaAPI.Models.Messaging;
using NeoNovaAPI.Services;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NeoNovaAPIAdmin.Controllers.MessagingControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PalantirMessagesController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public PalantirMessagesController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        //PALANTIR MESSAGE SECTION
        //Get
        [Authorize(Policy = "GeneralLeadership")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PalantirMessage>>> GetPalantirMessages()
        {
            string key = "palantirMessages";
            string cachedPalantirMessages = _redisService.GetString(key);

            if (cachedPalantirMessages != null)
            {
                return JsonConvert.DeserializeObject<List<PalantirMessage>>(cachedPalantirMessages);
            }

            var palantirMessages = await _context.PalantirMessages.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(palantirMessages), TimeSpan.FromMinutes(1));

            return palantirMessages;
        }
        //Get by Id
        [Authorize(Policy = "GeneralLeadership")]
        [HttpGet("{id}")]
        public async Task<ActionResult<PalantirMessage>> GetMessageById(int id)
        {
            string key = $"palantirMessage:{id}";
            string cachedPalantirMessage = _redisService.GetString(key);

            if (cachedPalantirMessage != null)
            {
                return JsonConvert.DeserializeObject<PalantirMessage>(cachedPalantirMessage);
            }

            var palantirMessage = await _context.PalantirMessages.FindAsync(id);

            if (palantirMessage == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(palantirMessage), TimeSpan.FromHours(1));

            return palantirMessage;
        }

        // POST: api/PalantírMessages
        //[Authorize(Policy = "GeneralLeadership")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> PostPalantírMessage(PalantirMessage newMessage)
        {
            try
            {
                _context.PalantirMessages.Add(newMessage);
                await _context.SaveChangesAsync();

                _redisService.DeleteKey("palantirMessages"); // Invalidate the cache

                return CreatedAtAction(nameof(GetMessageById), new { id = newMessage.Id }, newMessage);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"Database Error: {dbEx.Message}");
            }
        }

        //TAG SECTION
        // Nested routes for Tags
        [HttpGet("{messageId}/tags")]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTagsByMessage(int messageId)
        {
            string key = $"palantirMessage:{messageId}:tags";
            string cachedTags = _redisService.GetString(key);

            if (cachedTags != null)
            {
                return JsonConvert.DeserializeObject<List<Tag>>(cachedTags);
            }

            var tags = await _context.MessageTags
                                     .Where(mt => mt.PalantirMessageId == messageId)
                                     .Select(mt => mt.Tag)
                                     .ToListAsync();

            _redisService.SetString(key, JsonConvert.SerializeObject(tags), TimeSpan.FromHours(1));

            return tags;
        }

        // POST: api/PalantírMessages/{messageId}/tags
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("{messageId}/tags")]
        public async Task<IActionResult> PostTagForMessage(int messageId, Tag newTag)
        {
            try
            {
                var messageTag = new MessageTag { PalantirMessageId = messageId, Tag = newTag };
                _context.MessageTags.Add(messageTag);
                await _context.SaveChangesAsync();

                _redisService.DeleteKey($"palantirMessage:{messageId}:tags"); // Invalidate the cache

                return CreatedAtAction(nameof(GetTagsByMessage), new { messageId }, newTag);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"Database Error: {dbEx.Message}");
            }
        }

        //COMMENTS SECTION
        // Nested routes for Comments
        [HttpGet("{messageId}/comments")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByMessage(int messageId)
        {
            string key = $"palantirMessage:{messageId}:comments";
            string cachedComments = _redisService.GetString(key);

            if (cachedComments != null)
            {
                return JsonConvert.DeserializeObject<List<Comment>>(cachedComments);
            }

            var comments = await _context.Comments
                                         .Where(c => c.PalantirMessageId == messageId)
                                         .ToListAsync();

            _redisService.SetString(key, JsonConvert.SerializeObject(comments), TimeSpan.FromHours(1));

            return comments;
        }
        
        // POST: api/PalantírMessages/{messageId}/comments
        [Authorize(Policy = "AdminOnly")]
        [HttpPost("{messageId}/comments")]
        public async Task<IActionResult> PostCommentForMessage(int messageId, Comment newComment)
        {
            try
            {
                newComment.PalantirMessageId = messageId;
                _context.Comments.Add(newComment);
                await _context.SaveChangesAsync();

                _redisService.DeleteKey($"palantirMessage:{messageId}:comments"); // Invalidate the cache

                return CreatedAtAction(nameof(GetCommentsByMessage), new { messageId }, newComment);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"Database Error: {dbEx.Message}");
            }
        }

    }
}
