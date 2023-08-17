using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoNovaAPI.Models.UserModels;
using NeoNovaAPI.Services;
using Newtonsoft.Json;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NeoNovaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JwtService _jwtService;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, JwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var user = new IdentityUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "CommonUser");
                return Ok(new { Message = "Registration successful" });
            }
            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (result.Succeeded)
                {
                    // Sign in the user
                    await _signInManager.SignInAsync(user, false);

                    // Generate the JWT token
                    var token = await _jwtService.GenerateToken(user);

                    // Extract the user's name and roles from the token
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    var userName = jwtToken.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.UniqueName).Value;

                    // Set the JWT in a cookie
                    Response.Cookies.Append("MyCookieAuth", token, new CookieOptions { HttpOnly = true, Secure = true });

                    return Ok(new { token = token }); // Return the JWT token in the response
                }
            }
            return BadRequest("Invalid login attempt."); // Return a more descriptive error message
        }


        [Authorize(Roles = "Neo")]  // Only Neo can access this
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser(CreateUserModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Role != "Admin" && model.Role != "CommonUser")
                return BadRequest("Invalid role specified.");

            var user = new IdentityUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Assign the specified role to the user
            await _userManager.AddToRoleAsync(user, model.Role);

            return Ok(new { Message = $"User {model.Username} with role {model.Role} created successfully." });
        }

        [Authorize]
        [HttpPost("create-neo-user")]
        public async Task<IActionResult> CreateNeoUser()
        {
            var user = new IdentityUser { UserName = "TheNeoUser", Email = "neo@user.com", EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, "SecurePassword456!");

            if (result.Succeeded)
            {
                // Assign the "Neo" role
                await _userManager.AddToRoleAsync(user, "Neo");

                return Ok(new { Message = "Neo user created successfully" });
            }
            return BadRequest(result.Errors);
        }

        [Authorize]
        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }
    }
}
