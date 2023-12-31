﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NeoNovaAPI.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NeoNovaAPI.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly NeoNovaAPIDbContext _dbContext;



        public JwtService(NeoNovaAPIDbContext context, IConfiguration configuration, UserManager<IdentityUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
            _dbContext = context;
        }

        public async Task<string> GenerateToken(IdentityUser user)
        {
            var userName = user?.UserName;
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Invalid user or user name.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id), // User ID
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique Token ID
            };

            // Fetch SecurityUser details only if the role matches
            if (roles.Any(role => role == "SecurityChief" || role == "SecurityManager" || role == "SecuritySupervisor" || role == "SecurityOfficer"))
            {
                var securityUser = await _dbContext.SecurityUsers
                                          .FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);

                if (securityUser != null)
                {
                    if (!string.IsNullOrEmpty(securityUser.ID))
                    {
                        claims.Add(new Claim("ID", securityUser.ID));
                    }
                    if (!string.IsNullOrEmpty(securityUser.FirstName))
                    {
                        claims.Add(new Claim("FirstName", securityUser.FirstName));
                    }

                    if (!string.IsNullOrEmpty(securityUser.LastName))
                    {
                        claims.Add(new Claim("LastName", securityUser.LastName));
                    }

                    if (!string.IsNullOrEmpty(securityUser.SecurityUsername))
                    {
                        claims.Add(new Claim("SecurityUsername", securityUser.SecurityUsername));
                    }
                }
            }

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is missing from configuration.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Task<string> GeneratePasswordToken(string generatedPassword)
        {
            var claims = new List<Claim>
            {
                new Claim("GeneratedPassword", generatedPassword),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique Token ID
            };

            var jwtPasswordKey = _configuration["PasswordJwt:PasswordKey"];
            if (string.IsNullOrEmpty(jwtPasswordKey))
            {
                throw new InvalidOperationException("JWT Password Key is missing from configuration.");
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtPasswordKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["PasswordJwt:Issuer"],
                audience: _configuration["PasswordJwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds);

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }
    }
}
