namespace HabitTracker.Entities
{
    public class Habit
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Frequency { get; set; } // "daily" or "weekly"
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<HabitCompletion> Completions { get; set; } = new();
    }
}