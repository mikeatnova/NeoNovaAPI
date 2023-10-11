using NeoNovaAPI.Models.SecurityModels.Reporting;

    using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using global::NeoNovaAPI.Data;
using global::NeoNovaAPI.Services;

namespace NeoNovaAPI.Controllers.SecurityControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityLogController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public ActivityLogController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        // GET: api/ActivityLog
        [Authorize(Policy = "SecurityTeam")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActivityLog>>> GetActivityLogs()
        {
            string key = "activitylogs";
            string cachedLogs = _redisService.GetString(key);

            if (cachedLogs != null)
            {
                return JsonConvert.DeserializeObject<List<ActivityLog>>(cachedLogs);
            }

            var logs = await _context.ActivityLogs.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(logs), TimeSpan.FromDays(7));

            return logs;
        }

        // POST: api/ActivityLog
        [Authorize(Policy = "SecurityTeam")]
        [HttpPost]
        public async Task<ActionResult<ActivityLog>> PostActivityLog(ActivityLog activityLog)
        {
            _context.ActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("activitylogs"); // Invalidate the cache

            return CreatedAtAction("GetActivityLogs", new { id = activityLog.Id }, activityLog);
        }

        private bool ActivityLogExists(int id)
        {
            return (_context.ActivityLogs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
