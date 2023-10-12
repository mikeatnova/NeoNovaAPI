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
using NeoNovaAPI.Data;

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
        private readonly NeoNovaAPIDbContext _context;


        public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, JwtService jwtService, EmailService emailService, SeedUserGeneratorServices seedUserGenerator, NeoNovaAPIDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _emailService = emailService;
            _seedUserGenerator = seedUserGenerator;
            _context = context;
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

            bool isSecurityRole = (seedUser.Role == "SecurityOfficer" || seedUser.Role == "SecurityManager" || seedUser.Role == "SecuritySupervisor" || seedUser.Role == "SecurityChief");

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

                    if (isSecurityRole)
                    {
                        var securityUser = new SecurityUser
                        {
                            IdentityUserId = user.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            ModifiedAt = DateTime.UtcNow
                        };
                        _context.SecurityUsers.Add(securityUser);
                        await _context.SaveChangesAsync();
                    };
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

        [Authorize(Policy = "SecurityManagement")]
        [HttpGet("get-security-users")]
        public async Task<IActionResult> GetSecurityUsers()
        {
            try
            {
                // Fetch all users with their corresponding roles and SecurityUser details
                var usersWithRoles = await (from user in _context.Users
                                            join secUser in _context.SecurityUsers on user.Id equals secUser.IdentityUserId
                                            join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                            join role in _context.Roles on userRole.RoleId equals role.Id
                                            select new
                                            {
                                                user.Id,
                                                user.UserName,
                                                user.Email,
                                                user.PhoneNumber,
                                                secUser.FirstName,
                                                secUser.LastName,
                                                secUser.SecurityUsername,
                                                secUser.HiredDate,
                                                RoleName = role.Name
                                            })
                                            .ToListAsync();

                // Group by user and aggregate roles
                var groupedUsers = usersWithRoles.GroupBy(u => u.Id)
                                                 .Select(g => new
                                                 {
                                                     Id = g.Key,
                                                     UserName = g.First().UserName,
                                                     Email = g.First().Email,
                                                     PhoneNumber = g.First().PhoneNumber,
                                                     FirstName = g.First().FirstName,
                                                     LastName = g.First().LastName,
                                                     SecurityUsername = g.First().SecurityUsername,
                                                     HiredDate = g.First().HiredDate,
                                                     Roles = g.Select(u => u.RoleName).ToList()
                                                 })
                                                 .ToList();

                return Ok(groupedUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [Authorize(Policy = "SecurityTeam")]
        [HttpGet("get-security-user/{id}")]
        public async Task<IActionResult> GetSecurityUserById(string id)
        {
            try
            {
                var userWithRoles = await (from user in _context.Users
                                           join secUser in _context.SecurityUsers on user.Id equals secUser.IdentityUserId
                                           join userRole in _context.UserRoles on user.Id equals userRole.UserId
                                           join role in _context.Roles on userRole.RoleId equals role.Id
                                           where user.Id == id
                                           select new
                                           {
                                               user.Id,
                                               user.UserName,
                                               user.Email,
                                               user.PhoneNumber,
                                               secUser.FirstName,
                                               secUser.LastName,
                                               secUser.SecurityUsername,
                                               secUser.HiredDate,
                                               RoleName = role.Name
                                           })
                                           .ToListAsync();

                if (userWithRoles == null || !userWithRoles.Any())
                {
                    return NotFound($"User with ID {id} not found.");
                }

                var groupedUser = userWithRoles.GroupBy(u => u.Id)
                                               .Select(g => new
                                               {
                                                   Id = g.Key,
                                                   UserName = g.First().UserName,
                                                   Email = g.First().Email,
                                                   PhoneNumber = g.First().PhoneNumber,
                                                   FirstName = g.First().FirstName,
                                                   LastName = g.First().LastName,
                                                   SecurityUsername = g.First().SecurityUsername,
                                                   HiredDate = g.First().HiredDate,
                                                   Roles = g.Select(u => u.RoleName).ToList()
                                               })
                                               .First();

                return Ok(groupedUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }


        // Authorized only for Security Management
        [Authorize(Policy = "SecurityManagement")]
        [HttpPost("seed-new-security-user")]
        public async Task<IActionResult> SeedNewSecurityUser([FromBody] SeedSecurityUserModel seedUser)
        {
            // Validation for 'Role' if needed
            if (!IsValidSecurityRole(seedUser.Role)) return BadRequest("Invalid Role");

            string username = string.Empty;
            string password = seedUser.Password;
            IdentityResult result;

            do
            {
                // Generate a username based on the role
                username = _seedUserGenerator.SeedUsernameGenerator(seedUser.Role);

                // Create IdentityUser
                var user = new IdentityUser
                {
                    UserName = username,
                    Email = seedUser.Email,
                    PhoneNumber = seedUser.PhoneNumber,
                    EmailConfirmed = true
                };

                // Create User
                result = await _userManager.CreateAsync(user, seedUser.Password);

                if (result.Succeeded)
                {
                    // Assign Role
                    await _userManager.AddToRoleAsync(user, seedUser.Role);

                    // Create SecurityUser entity
                    var securityUser = new SecurityUser
                    {
                        IdentityUserId = user.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow,
                        FirstName = seedUser.FirstName,
                        LastName = seedUser.LastName,
                        HiredDate = seedUser.HiredDate
                    };

                    _context.SecurityUsers.Add(securityUser);
                    await _context.SaveChangesAsync();

                    return Ok(new { Message = $"{seedUser.Role} user created successfully", Username = username });
                }

            } while (result.Errors.Any(e => e.Code == "DuplicateUserName"));

            return BadRequest(result.Errors);
        }

        private bool IsValidSecurityRole(string role)
        {
            return role == "SecurityOfficer" || role == "SecurityManager" || role == "SecuritySupervisor" || role == "SecurityChief";
        }


        [Authorize(Policy = "SecurityManagement")]
        [HttpPut("update-security-user")]
        public async Task<IActionResult> UpdateSecurityUser([FromBody] UpdateSecurityUserDto updateDto)
        {
            try
            {
                // Fetch the Identity User
                var identityUser = await _userManager.FindByIdAsync(updateDto.Id);
                if (identityUser == null)
                {
                    return NotFound("User not found");
                }

                // Update Identity User details
                identityUser.Email = updateDto.Email;
                identityUser.PhoneNumber = updateDto.PhoneNumber;
                var identityResult = await _userManager.UpdateAsync(identityUser);

                if (!identityResult.Succeeded)
                {
                    return BadRequest(identityResult.Errors);
                }

                // Fetch the Security User
                var securityUser = await _context.SecurityUsers
                                                  .FirstOrDefaultAsync(s => s.IdentityUserId == updateDto.Id);

                if (securityUser == null)
                {
                    return NotFound("Security User not found");
                }

                // Update Security User details
                securityUser.FirstName = updateDto.FirstName;
                securityUser.LastName = updateDto.LastName;
                securityUser.SecurityUsername = updateDto.SecurityUsername;
                securityUser.HiredDate = updateDto.HiredDate ?? securityUser.HiredDate;

                _context.SecurityUsers.Update(securityUser);
                await _context.SaveChangesAsync();

                return Ok("User updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [Authorize]
        [HttpGet("get-user-profile/{id}")]
        public async Task<IActionResult> GetUserProfile([FromRoute] string id)
        {
            // Fetch the IdentityUser from the database
            var identityUser = await _userManager.FindByIdAsync(id);
            if (identityUser == null)
            {
                return NotFound("IdentityUser not found.");
            }

            // Fetch the associated SecurityUser from the database
            var securityUser = await _context.SecurityUsers
                                       .FirstOrDefaultAsync(su => su.IdentityUserId == id);
            if (securityUser == null)
            {
                return NotFound("SecurityUser not found.");
            }

            // Prepare the data for the client
            var profileData = new
            {
                IdentityUser = identityUser,
                SecurityUser = securityUser
            };

            return Ok(profileData);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return BadRequest("User not found.");
                }

                // Query for an associated SecurityUser, if any
                var securityUser = await _context.SecurityUsers
                                    .Where(s => s.IdentityUserId == id)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();

                // If SecurityUser exists, remove it
                if (securityUser != null)
                {
                    _context.SecurityUsers.Remove(securityUser);
                    await _context.SaveChangesAsync();
                }

                var identityDeleteResult = await _userManager.DeleteAsync(user);
                if (!identityDeleteResult.Succeeded)
                {
                    transaction.Rollback();
                    return BadRequest($"Failed to delete AspNetUser: {string.Join(", ", identityDeleteResult.Errors.Select(e => e.Description))}");
                }

                transaction.Commit();
                return Ok("User and associated SecurityUser deleted successfully.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
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
