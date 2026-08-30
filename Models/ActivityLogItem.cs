namespace FlexiFit_AdminPanel.Models
{
    public class ActivityLogItem
    {
        public int user_id { get; set; }
        public string? username { get; set; }
        public string? email { get; set; }
        public string? activity_type { get; set; }
        public DateTime activity_date { get; set; }
        public string? details { get; set; }
    }
}