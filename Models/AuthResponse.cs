namespace FlexiFit_AdminPanel.Models
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public string? Name { get; set; }
        public string? PhotoUrl { get; set; }
    }
}