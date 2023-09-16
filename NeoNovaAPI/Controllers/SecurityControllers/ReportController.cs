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
using NeoNovaAPI.Models.SecurityModels.Reporting;

namespace NeoNovaAPI.Controllers.SecurityControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public ReportController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        // GET: api/Reports
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Report>>> GetReports()
        {
            string key = "reports";
            string cachedReports = _redisService.GetString(key);

            if (cachedReports != null)
            {
                return JsonConvert.DeserializeObject<List<Report>>(cachedReports);
            }

            var reports = await _context.Reports.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(reports), TimeSpan.FromDays(7));

            return reports;
        }

        // GET: api/Reports/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Report>> GetReport(int id)
        {
            string key = $"report:{id}";
            string cachedReport = _redisService.GetString(key);

            if (cachedReport != null)
            {
                return JsonConvert.DeserializeObject<Report>(cachedReport);
            }

            var report = await _context.Reports.FindAsync(id);

            if (report == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(report), TimeSpan.FromDays(7));

            return report;
        }

        // PUT: api/Reports/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReport(int id, Report report)
        {
            if (id != report.ID)
            {
                return BadRequest();
            }

            _context.Entry(report).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("reports");
            _redisService.DeleteKey($"report:{id}");

            return NoContent();
        }

        // POST: api/Reports
        [Authorize(Policy = "SecurityTeam")]
        [HttpPost]
        public async Task<ActionResult<Report>> PostReport(Report report)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("reports");

            return CreatedAtAction("GetReports", new { id = report.ID }, report);
        }

        // DELETE: api/Reports/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("reports");
            _redisService.DeleteKey($"report:{id}");

            return NoContent();
        }

        private bool ReportExists(int id)
        {
            return (_context.Reports?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
