using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NeoNovaAPI.Data;
using NeoNovaAPI.Services;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);



if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Roles
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.User.RequireUniqueEmail = true;
})
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<NeoNovaAPIDbContext>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp",
        builder =>
        {
            builder.WithOrigins(
                "https://localhost:7164",
                "https://novawholesalema.com",
                "http://localhost:4000",
                "https://neonovaadmin.azurewebsites.net"
            )
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

// Add JWT Authentication for Identity
var jwtSettings = builder.Configuration.GetSection("Jwt");

// Ensure JWT Key is not null
var jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.AddPolicy("NeoOnly", policy =>
        policy.RequireRole("Neo"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Neo", "Admin"));
    
    options.AddPolicy("GeneralLeadership", policy =>
        policy.RequireRole("Neo", "Admin", "SecurityChief", "SecurityManager", "SecuritySupervisor"));

    options.AddPolicy("AllUsers", policy =>
        policy.RequireRole( "Neo", "Admin", "CommonUser"));

    options.AddPolicy("SecurityChiefOnly", policy =>
        policy.RequireRole("Neo", "Admin", "SecurityChief"));

    options.AddPolicy("SecurityManagement", policy =>
    policy.RequireRole("Neo", "Admin", "SecurityChief", "SecurityManager"));

    options.AddPolicy("SecuritySupervisor", policy =>
    policy.RequireRole("Neo", "Admin", "SecurityChief", "SecurityManager", "SecuritySupervisor"));

    options.AddPolicy("SecurityTeam", policy =>
    policy.RequireRole("Neo", "Admin", "SecurityChief", "SecurityManager", "SecuritySupervisor", "SecurityOfficer"));
});


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
    };
});

builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
    opt.TokenLifespan = TimeSpan.FromMinutes(15)); 



// Add DbContext
string connectionString = builder.Configuration.GetConnectionString("AzureSQLDb") ?? throw new InvalidOperationException("Could not find a connection string named 'AzureSQLDb'.");
builder.Services.AddDbContext<NeoNovaAPIDbContext>(options => options.UseSqlServer(connectionString));

// Add Redis Context
var redisSettings = builder.Configuration.GetSection("Redis");
var redisConnectionString = redisSettings["ConnectionString"] ?? throw new InvalidOperationException("Redis connection string is not configured.");
builder.Services.AddSingleton(x => ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddTransient<RedisService>();
builder.Services.AddTransient<JwtService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddTransient<SeedUserGeneratorServices>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowWebApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
