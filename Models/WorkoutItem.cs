using System;
using System.ComponentModel.DataAnnotations;

namespace FlexiFit_AdminPanel.Models
{
    public class WorkoutItem
    {
        [Key]
        public int workout_id { get; set; }
        public string workout_name { get; set; } = string.Empty;
        public string muscle_group { get; set; } = string.Empty;
        public string equipment { get; set; } = string.Empty;
        public string environment { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty; // Halimbawa: MUSCLE_GAIN, WARMUP, REHAB
        public string difficulty_level { get; set; } = string.Empty;
        public int is_weighted { get; set; }
        public string? notes { get; set; }
        public int calories_burned { get; set; }
        public int is_active { get; set; }
        public DateTime created_at { get; set; }    
        public DateTime updated_at { get; set; }
        public string? img_filename { get; set; }
        public int duration { get; set; }

        // --- DITO ANG REVISON PARA SA IMAGES ---

        /// <summary>
        /// Awtomatikong binubuo ang URL ng image galing sa API base sa category subfolders.
        /// </summary>
        public string FullImageUrl
        {
            get
            {
                var apiBaseUrl = "http://localhost:5160";

                // Fallback kapag walang filename sa database
                if (string.IsNullOrEmpty(img_filename))
                    return $"{apiBaseUrl}/images/workouts/default.png";

                string folderPath = "images/workouts"; // Default folder
                string cat = category?.ToUpper() ?? "";

                // Tumpak na mapping base sa iyong file explorer
                if (cat.Contains("MUSCLE_GAIN"))
                {
                    folderPath = "images/workouts/muscle_gain";
                }
                else if (cat.Contains("CARDIO") || cat.Contains("WARMUP"))
                {
                    folderPath = "images/workout/cardio";
                }
                else if (cat.Contains("REHAB"))
                {
                    folderPath = "images/workouts/rehab";
                }
                else
                {
                    folderPath = "images/workouts/muscle_gain";
                }

                return $"{apiBaseUrl}/{folderPath}/{img_filename}";
            }
        }
    }
}