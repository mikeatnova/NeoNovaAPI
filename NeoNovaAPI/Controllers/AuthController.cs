using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoNovaAPI.Models.UserModels;
using NeoNovaAPI.Services;
using System.Net.Mail;
using System.Net;
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
        private readonly EmailService _emailService;

        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, JwtService jwtService, EmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        [AllowAnonymous]
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

        [AllowAnonymous]
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
                    // Generate the JWT token
                    var token = await _jwtService.GenerateToken(user);

                    // Extract the user's name and roles from the token
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    var userId = jwtToken.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value; // Using 'sub' for user ID

                    // Set the JWT in a cookie
                    Response.Cookies.Append("MyCookieAuth", token, new CookieOptions { HttpOnly = true, Secure = true });

                    return Ok(new { token = token }); // Return the JWT token in the response
                }
            }
            return BadRequest("Invalid login attempt."); // Return a more descriptive error message
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BadRequest("Email is required.");
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Ok(new { Message = "If the email address exists, a password reset token has been sent." });
            }

            // Generate password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Compose email with the resetToken
            var subject = "Password Reset";
            var body = $"Your password reset token is: {resetToken}";
            await _emailService.SendEmailAsync(model.Email, subject, body);

            return Ok(new { Message = "If the email address exists, a password reset token has been sent." });
        }

        [AllowAnonymous]
        [HttpPost("validate-reset-code")]
        public async Task<IActionResult> ValidateAndSetNewPassword([FromBody] ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.NewPassword);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Password successfully reset" });
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [Authorize(Policy = "NeoOnly")]
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

        [Authorize(Policy = "NeoOnly")]
        [HttpPost("create-common-user")]
        public async Task<IActionResult> CreateCommonUser()
        {
            var user = new IdentityUser { UserName = "TheCommonUser", Email = "common@user.com", EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, "CommonUserCode123!");

            if (result.Succeeded)
            {
                // Assign the "CommonUser" role
                await _userManager.AddToRoleAsync(user, "CommonUser");

                return Ok(new { Message = "Common user created successfully" });
            }
            return BadRequest(result.Errors);
        }

        [Authorize(Policy = "NeoOnly")]
        [HttpPost("create-admin-user")]
        public async Task<IActionResult> CreateAdminUser()
        {
            var user = new IdentityUser { UserName = "TheAdminUser", Email = "admin@user.com", EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, "AdminCode789!");

            if (result.Succeeded)
            {
                // Assign the "Admin" role
                await _userManager.AddToRoleAsync(user, "Admin");

                return Ok(new { Message = "Admin user created successfully" });
            }
            return BadRequest(result.Errors);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }
    }
}
