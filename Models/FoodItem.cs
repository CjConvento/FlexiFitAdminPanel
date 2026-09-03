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

    }
}