using System.ComponentModel.DataAnnotations;

namespace HabitTracker.ViewModels
{
    public class UpdateHabitViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Frequency { get; set; } = "daily";
    }
}