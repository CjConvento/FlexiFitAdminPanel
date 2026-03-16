using System;

namespace FlexiFit_AdminPanel.Models
{
    public class User
    {
        // Primary Key - Auto-increment sa SQL
        public int user_id { get; set; }

        // Mula sa Firebase (Google UID o Manual ID)
        public string? firebase_uid { get; set; }

        // Full Name ng User
        public string? name { get; set; }

        public string username { get; set; } = string.Empty;

        public string email { get; set; } = string.Empty;

        // E.g., Admin, User
        public string role { get; set; } = "USER";

        // Automatic set sa SQL: active / inactive
        public string status { get; set; } = "active";

        // Automatic set sa SQL: true/false
        public bool is_verified { get; set; } = true;

        // E.g., manual, google
        public string auth_provider { get; set; } = "EMAIL";

        // Timestamps
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime updated_at { get; set; } = DateTime.Now;
    }
}