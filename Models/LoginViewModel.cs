namespace FlexiFit_AdminPanel.Models
{
    public class LoginViewModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FirebaseToken { get; set; }
        public bool RememberMe { get; set; }
    }
}