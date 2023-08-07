using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoNovaAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeedNeoRoleAndUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed roles
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1", "Neo", "NEO" },
                    { "2", "Admin", "ADMIN" },
                    { "9", "CommonUser", "COMMONUSER" }
                });

            // Seed the Neo user
            var neoUserId = Guid.NewGuid().ToString();
            var hasher = new PasswordHasher<IdentityUser>();
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                                 "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumberConfirmed",
                                 "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount", "EmailConfirmed" },
                values: new object[]
                {
                    neoUserId,
                    "neoKsama",
                    "NEOKSAMA",
                    "mcognata@novafarms.com",
                    "MCOGNATA@NOVAFARMS.COM",
                    hasher.HashPassword(null, "NeoTempPass123!"),
                    Guid.NewGuid().ToString(), // SecurityStamp
                    Guid.NewGuid().ToString(), // ConcurrencyStamp
                    false, // PhoneNumberConfirmed
                    false, // TwoFactorEnabled
                    true,  // LockoutEnabled
                    0,    // AccessFailedCount
                    true  // EmailConfirmed
                });

            // Assign the Neo role to Neo user
            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[] { neoUserId, "1" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
