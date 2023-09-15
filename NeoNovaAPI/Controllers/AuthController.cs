﻿using Microsoft.AspNetCore.Authorization;
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
        private readonly SeedUserGeneratorServices _seedUserGenerator;


        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, JwtService jwtService, EmailService emailService, SeedUserGeneratorServices seedUserGenerator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _emailService = emailService;
            _seedUserGenerator = seedUserGenerator;
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
        [HttpPost("seed-new-user")]
        public async Task<IActionResult> SeedNewUser([FromBody] SeedNewUserModel seedUser)
        {
            string password = seedUser.Password; // Get the password from the request
            string username = string.Empty;
            IdentityResult result;

            do
            {
                username = _seedUserGenerator.SeedUsernameGenerator(seedUser.Role);

                var user = new IdentityUser
                {
                    UserName = username,
                    Email = seedUser.Email,
                    EmailConfirmed = true
                };

                result = await _userManager.CreateAsync(user, password); // Use the password from the request

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, seedUser.Role);
                    return Ok(new { Message = $"{seedUser.Role} user created successfully", Username = username });
                }

            } while (result.Errors.Any(e => e.Code == "DuplicateUserName"));

            return BadRequest(result.Errors);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet("get-users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();

                var userDTOs = users.Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    Roles = _userManager.GetRolesAsync(u).Result  // Sync-over-async for simplification; consider using proper async handling
                }).ToList();

                return Ok(userDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            try
            {
                // Find the user by ID
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return BadRequest("User not found.");
                }

                // Delete the user
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return Ok("User deleted successfully.");
                }
                else
                {
                    return BadRequest($"Failed to delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }


        [Authorize(Policy = "AdminOnly")]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetUserPassword([FromBody] ChangeUserPasswordModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            // Generate a new password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Password reset successfully." });
            }
            return BadRequest(result.Errors);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost("reset-username")]
        public async Task<IActionResult> ResetUserUsername([FromBody] ChangeUserUsernameModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return BadRequest("User not found");
            }
            user.UserName = model.NewUsername;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                string newToken = await _jwtService.GenerateToken(user);
                return Ok(new {Token = newToken });
            }
            return BadRequest(result.Errors);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost("{id}/validate-password")]
        public async Task<IActionResult> ValidatePassword([FromRoute] string id, [FromBody] ValidatePasswordModel model)
        {
            // Since user ID is coming from the route, make sure it matches the one in the body
            if (id != model.UserId) return BadRequest("Mismatched user IDs");

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return BadRequest("User not found");

            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);

            return Ok(new { IsValid = isCurrentPasswordValid });
        }
    }
}
