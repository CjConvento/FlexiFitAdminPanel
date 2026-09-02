namespace FlexiFit_AdminPanel.Models
{
    public class ActivityLogItem
    {
        public int user_id { get; set; }
        public string? username { get; set; }
        public string? email { get; set; }
        public string? activity_type { get; set; }
        public DateTime? activity_date { get; set; }
        public string? details { get; set; }
        public int calories_burned { get; set; }
        public int total_minutes { get; set; }
        public DateTime log_date { get; set; }
    }

    // ✅ Para sa paginated response mula sa API
    public class ActivityLogResponse
    {
        public List<ActivityLogItem> Data { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}