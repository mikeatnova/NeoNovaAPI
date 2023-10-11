using Microsoft.AspNetCore.Identity;
using NeoNovaAPI.Models.UserModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace NeoNovaAPI.Models.SecurityModels.Reporting
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string? SecurityUserId { get; set; }
        public string? IdentityUserId { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserRole { get; set; }
        public string? ActivityType { get; set; }
        public string? Action { get; set; }
        public DateTime ActionTime { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("SecurityUserId")]
        public SecurityUser? SecurityUser { get; set; }

        [ForeignKey("IdentityUserId")]
        public IdentityUser? IdentityUser { get; set; }
    }
}
