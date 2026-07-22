namespace HabitTracker.ViewModels
{
    public class HabitDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public int Streak { get; set; }
        public bool CompletedToday { get; set; }
        public List<DayStatusDto> Last7Days { get; set; } = new();
    }

    public class DayStatusDto
    {
        public string Date { get; set; } = string.Empty;
        public string DayLabel { get; set; } = string.Empty;
        public bool Completed { get; set; }
    }

    public class WeekResponseDto
    {
        public List<DayStatusDto> Days { get; set; } = new();
        public string RangeLabel { get; set; } = string.Empty;
        public int Offset { get; set; }
    }

    public class HabitDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TotalCompletions { get; set; }
        public double CompletionRate { get; set; }
        public List<string> CompletionDates { get; set; } = new();
    }
}