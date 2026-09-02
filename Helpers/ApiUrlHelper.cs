using Microsoft.Extensions.Configuration;

namespace FlexiFit_AdminPanel.Helpers
{
    public static class ApiUrlHelper
    {
        private static string? _baseUrl;

        public static void Configure(IConfiguration configuration)
        {
            _baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("API Base URL not configured in ApiSettings:BaseUrl.");
        }

        public static string BaseUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_baseUrl))
                {
                    throw new InvalidOperationException("ApiUrlHelper has not been configured. Call ApiUrlHelper.Configure() in Program.cs.");
                }
                return _baseUrl;
            }
        }
    }
}