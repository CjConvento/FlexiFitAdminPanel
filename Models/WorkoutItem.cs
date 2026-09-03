using System;
using System.ComponentModel.DataAnnotations;
using FlexiFit_AdminPanel.Helpers;
using System.Text.Json.Serialization;

namespace FlexiFit_AdminPanel.Models
{
    public class WorkoutItem
    {
        [JsonPropertyName("workoutId")]
        public int workout_id { get; set; }

        [JsonPropertyName("workoutName")]
        public string workout_name { get; set; } = string.Empty;

        [JsonPropertyName("muscleGroup")]
        public string? muscle_group { get; set; }

        [JsonPropertyName("equipment")]
        public string? equipment { get; set; }

        [JsonPropertyName("environment")]
        public string? environment { get; set; }

        [JsonPropertyName("category")]
        public string? category { get; set; }

        [JsonPropertyName("difficultyLevel")]
        public string? difficulty_level { get; set; }

        [JsonPropertyName("isWeighted")]
        public bool is_weighted { get; set; }

        [JsonPropertyName("notes")]
        public string? notes { get; set; }

        [JsonPropertyName("caloriesBurned")]
        public int? calories_burned { get; set; }

        [JsonPropertyName("isActive")]
        public bool is_active { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime created_at { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime updated_at { get; set; }

        [JsonPropertyName("imgFilename")]
        public string? img_filename { get; set; }

        [JsonPropertyName("duration")]
        public int? duration { get; set; }

        [JsonPropertyName("videoUrl")]
        public string? video_url { get; set; }

        // --- DITO ANG REVISON PARA SA IMAGES ---

        /// <summary>
        /// Awtomatikong binubuo ang URL ng image galing sa API base sa category subfolders.
        /// </summary>
        public string FullImageUrl
        {
            get
            {
                var apiBaseUrl = ApiUrlHelper.BaseUrl;

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
                    folderPath = "images/workouts/cardio";
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