namespace FlexiFit_AdminPanel.Models
{
    public class UserSession
    {
        public string Username { get; set; }
        public string Role { get; set; }
        public string Token { get; set; } // Optional if you use JWT
    }
}