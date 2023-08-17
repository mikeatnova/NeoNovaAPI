using Microsoft.AspNetCore.Authentication.JwtBearer;
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
builder.Services.AddDefaultIdentity<IdentityUser>()
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<NeoNovaAPIDbContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp",
        builder =>
        {
            builder.WithOrigins("https://neonovaadmin.azurewebsites.net") // Replace with your web app's origin
                   .AllowCredentials()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Add JWT Authentication for Identity
var jwtSettings = builder.Configuration.GetSection("Jwt");

// Ensure JWT Key is not null
var jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");

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
        ValidAudience = jwtSettings["Audiences"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
    };
});


// Add DbContext
string connectionString = builder.Configuration.GetConnectionString("AzureSQLDb") ?? throw new InvalidOperationException("Could not find a connection string named 'AzureSQLDb'.");
builder.Services.AddDbContext<NeoNovaAPIDbContext>(options => options.UseSqlServer(connectionString));

// Add Redis Context
var redisSettings = builder.Configuration.GetSection("Redis");
var redisConnectionString = redisSettings["ConnectionString"] ?? throw new InvalidOperationException("Redis connection string is not configured.");
builder.Services.AddSingleton(x => ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddTransient<RedisService>();
builder.Services.AddTransient<JwtService>();

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
