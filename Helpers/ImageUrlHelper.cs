namespace FlexiFit_AdminPanel.Helpers;

public static class ImageUrlHelper
{
    private static string? _apiBaseUrl;    // Para sa default images
    private static string? _blobBaseUrl;   // Para sa actual images (Azure)

    private static readonly Dictionary<string, string> _defaultImages = new()
    {
        { "workouts", "/images/workouts/default.png" },
        { "foods", "/images/foods/default.png" },
        { "avatars", "/uploads/avatars/default.png" }
    };
    private static readonly string _fallbackDefault = "/images/default.png";

    // ✅ BAGONG CONFIGURE: Dalawang parameters
    public static void Configure(string apiBaseUrl, string blobBaseUrl)
    {
        _apiBaseUrl = apiBaseUrl?.TrimEnd('/');
        _blobBaseUrl = blobBaseUrl?.TrimEnd('/');
    }

    public static string GetFullImageUrl(string fileName, string container)
    {
        // 🔒 SAFETY: Kung wala pang laman ang _blobBaseUrl, mag-fallback
        if (string.IsNullOrEmpty(_blobBaseUrl))
            _blobBaseUrl = "https://flexifitstorage.blob.core.windows.net";

        // ✅ KUNG WALANG IMAGE (null or empty) → Return default mula sa API
        if (string.IsNullOrEmpty(fileName))
            return GetDefaultImage(container);

        // ✅ KUNG FULL URL NA (galing sa API or iba) → Diretsohin na lang
        if (fileName.StartsWith("http://") || fileName.StartsWith("https://"))
            return fileName;

        // ✅ ACTUAL IMAGE → Galing sa Azure Blob
        return $"{_blobBaseUrl}/{container}/{fileName}";
    }

    public static string GetDefaultImage(string container)
    {
        // 🔒 SAFETY: Kung wala pang laman ang _apiBaseUrl, mag-fallback
        if (string.IsNullOrEmpty(_apiBaseUrl))
            _apiBaseUrl = "https://flexifit-api-bqdrdcchf8faagat.japaneast-01.azurewebsites.net"; // Palitan ng API URL mo

        var defaultPath = _defaultImages.TryGetValue(container?.ToLower() ?? "", out var path) 
            ? path 
            : _fallbackDefault;
        
        return $"{_apiBaseUrl}{defaultPath}";
    }
}