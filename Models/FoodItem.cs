using System.ComponentModel.DataAnnotations;

namespace FlexiFit_AdminPanel.Models
{
    public class FoodItem
    {
        [Key]
        public int food_id { get; set; }
        public string food_name { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty;
        public decimal calories { get; set; }
        public decimal protein { get; set; }
        public decimal carbs { get; set; }
        public decimal fats { get; set; }

        // Mahalaga para sa image mapping sa API
        public string? img_filename { get; set; }

        // --- DAGDAG PARA SA DRAFT MODE & AUDIT ---

        // Gagamitin ito ng JS para sa Active/Inactive filtering
        public int is_active { get; set; }

        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }


        // --- DITO ILALAGAY ANG DYNAMIC URL LOGIC ---
        public string FullImageUrl
        {
            get
            {
                var apiBaseUrl = "http://localhost:5160";

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