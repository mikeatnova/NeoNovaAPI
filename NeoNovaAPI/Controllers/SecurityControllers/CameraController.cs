﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NeoNovaAPI.Data;
using NeoNovaAPI.Services;
using Newtonsoft.Json;
using NeoNovaAPI.Models.SecurityModels.CameraManagement;
using Microsoft.AspNetCore.JsonPatch;

namespace NeoNovaAPI.Controllers.SecurityControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CameraController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;
        private readonly RedisService _redisService;

        public CameraController(NeoNovaAPIDbContext context, RedisService redisService)
        {
            _context = context;
            _redisService = redisService;
        }

        // GET: api/Cameras
        //[AllowAnonymous]
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Camera>>> GetCameras()
        {
            string key = "cameras";
            string cachedCameras = _redisService.GetString(key);

            if (cachedCameras != null)
            {

                return JsonConvert.DeserializeObject<List<Camera>>(cachedCameras);
            }

            var cameras = await _context.Cameras.ToListAsync();
            _redisService.SetString(key, JsonConvert.SerializeObject(cameras), TimeSpan.FromDays(1));

            return cameras;
        }

        // GET: api/Cameras/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Camera>> GetCamera(int id)
        {
            string key = $"camera:{id}";
            string cachedCamera = _redisService.GetString(key);

            if (cachedCamera != null)
            {
                return JsonConvert.DeserializeObject<Camera>(cachedCamera);
            }

            var camera = await _context.Cameras.FindAsync(id);

            if (camera == null)
            {
                return NotFound();
            }

            _redisService.SetString(key, JsonConvert.SerializeObject(camera), TimeSpan.FromDays(1));

            return camera;
        }

        // PUT: api/Cameras/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCamera(int id, Camera camera)
        {
            if (id != camera.ID)
            {
                return BadRequest();
            }

            _context.Entry(camera).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CameraExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            _redisService.DeleteKey("cameras"); // Invalidate the cache
            _redisService.DeleteKey($"camera:{id}"); // Invalidate the specific cache entry

            return NoContent();
        }

        // POST: api/Cameras
        [Authorize(Policy = "SecurityTeam")]
        [HttpPost]
        public async Task<ActionResult<Camera>> PostCamera(Camera camera)
        {
            _context.Cameras.Add(camera);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("cameras"); // Invalidate the cache

            return CreatedAtAction("GetCameras", new { id = camera.ID }, camera);
        }

        // PATCH: api/Cameras
        [Authorize(Policy = "SecurityTeam")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchCamera(int id, [FromBody] JsonPatchDocument<Camera> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            string key = $"camera:{id}";
            string cachedCamera = _redisService.GetString(key);
            Camera camera;

            if (cachedCamera != null)
            {
                camera = JsonConvert.DeserializeObject<Camera>(cachedCamera);
            }
            else
            {
                camera = await _context.Cameras.FindAsync(id);
                if (camera == null)
                {
                    return NotFound();
                }
            }

            patchDoc.ApplyTo(camera, (Microsoft.AspNetCore.JsonPatch.Adapters.IObjectAdapter)ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(camera).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("cameras"); // Invalidate the cache
            _redisService.DeleteKey(key); // Invalidate the specific cache entry

            return NoContent();
        }

        // DELETE: api/Cameras/5
        [Authorize(Policy = "SecurityTeam")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCamera(int id)
        {
            var camera = await _context.Cameras.FindAsync(id);
            if (camera == null)
            {
                return NotFound();
            }

            _context.Cameras.Remove(camera);
            await _context.SaveChangesAsync();

            _redisService.DeleteKey("cameras"); // Invalidate the cache
            _redisService.DeleteKey($"camera:{id}"); // Invalidate the specific cache entry

            return NoContent();
        }

        private bool CameraExists(int id)
        {
            return (_context.Cameras?.Any(e => e.ID == id)).GetValueOrDefault();
        }
    }
}
