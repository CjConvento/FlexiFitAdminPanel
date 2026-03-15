namespace FlexiFit_AdminPanel.Models
{
    public class FoodItem
    {
        public int food_id { get; set; }
        public string food_name { get; set; }
        public string category { get; set; }
        public decimal calories { get; set; }
        public decimal protein { get; set; }
        public decimal carbs { get; set; }
        public decimal fats { get; set; }
    }
}