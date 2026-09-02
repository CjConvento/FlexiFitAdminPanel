using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;  
using FlexiFit_AdminPanel.Helpers;

namespace FlexiFit_AdminPanel.Models
{
    public class FoodItem
    {
        [JsonPropertyName("foodId")]
        public int food_id { get; set; }

        [JsonPropertyName("foodName")]
        public string food_name { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string category { get; set; } = string.Empty;

        [JsonPropertyName("calories")]
        public decimal calories { get; set; }

        [JsonPropertyName("proteinG")]
        public decimal protein { get; set; }

        [JsonPropertyName("carbsG")]
        public decimal carbs { get; set; }

        [JsonPropertyName("fatsG")]
        public decimal fats { get; set; }

        [JsonPropertyName("imgFilename")]
        public string? img_filename { get; set; }

        [JsonPropertyName("isActive")]
        public bool is_active { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime created_at { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime updated_at { get; set; }


        // --- DITO ILALAGAY ANG DYNAMIC URL LOGIC ---
        public string FullImageUrl
        {
            get
            {
                var apiBaseUrl = ApiUrlHelper.BaseUrl;

                // 1. Kung walang filename sa DB, ituro agad sa API default image
                if (string.IsNullOrWhiteSpace(img_filename))
                {
                    return $"{apiBaseUrl}/images/foods/default.png";
                }

                string fn = img_filename.ToLower();
                string categoryFolder = "vegan";
                string typeFolder = "lunch";

                // 2. Mapping para sa Main Category Folders
                if (fn.Contains("keto")) categoryFolder = "keto";
                else if (fn.Contains("vegan")) categoryFolder = "vegan";
                else if (fn.Contains("vegetarian")) categoryFolder = "vegetarian";
                else if (fn.Contains("lactose_free") || fn.Contains("lactose-free")) categoryFolder = "lactose_free";
                else if (fn.Contains("high_protein") || fn.Contains("high-protein")) categoryFolder = "high_protein";
                else if (fn.Contains("balanced")) categoryFolder = "balanced";
                else categoryFolder = "vegan"; // Default category

                // 3. Mapping para sa Subfolders (Meal Type)
                if (fn.Contains("breakfast")) typeFolder = "breakfast";
                else if (fn.Contains("lunch")) typeFolder = "lunch";
                else if (fn.Contains("dinner")) typeFolder = "dinner";
                else if (fn.Contains("snack")) typeFolder = "snacks";
                else typeFolder = "lunch"; // Default type

                // 4. Pagbuo ng Final URL
                return $"{apiBaseUrl}/images/foods/{categoryFolder}/{typeFolder}/{img_filename}";
            }
        }
    }
}