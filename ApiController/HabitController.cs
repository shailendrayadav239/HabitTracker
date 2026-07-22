using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HabitTracker.ApplicationDBContext;
using HabitTracker.Entities;
using HabitTracker.ViewModels;

namespace HabitTracker.ApiController
{
    [Route("api/habits")]
    [ApiController]
    public class HabitController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HabitController(AppDbContext context)
        {
            _context = context;
        }

        private Guid? GetUserId()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return null;
            return Guid.Parse(userIdStr);
        }

        [HttpGet]
        public async Task<IActionResult> GetHabits()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var habits = await _context.Habits
                .Where(h => h.UserId == userId)
                .Include(h => h.Completions)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var result = habits.Select(h => BuildHabitDto(h, today)).ToList();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddHabit([FromBody] AddHabitViewModel model)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var habit = new Habit
            {
                Id = Guid.NewGuid(),
                Name = model.Name.Trim(),
                Frequency = model.Frequency,
                UserId = userId.Value,
                CreatedAt = DateTime.UtcNow
            };

            _context.Habits.Add(habit);
            await _context.SaveChangesAsync();

            var today = DateOnly.FromDateTime(DateTime.Now);
            return Ok(BuildHabitDto(habit, today));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHabit(Guid id, [FromBody] UpdateHabitViewModel model)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var habit = await _context.Habits
                .Include(h => h.Completions)
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (habit == null) return NotFound();

            habit.Name = model.Name.Trim();
            habit.Frequency = model.Frequency;
            await _context.SaveChangesAsync();

            var today = DateOnly.FromDateTime(DateTime.Now);
            return Ok(BuildHabitDto(habit, today));
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteHabit(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var habit = await _context.Habits
                .Include(h => h.Completions)
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (habit == null) return NotFound();

            var today = DateOnly.FromDateTime(DateTime.Now);

            if (habit.Completions.Any(c => c.Date == today))
                return BadRequest(new { message = "Already marked complete for today." });

            _context.HabitCompletions.Add(new HabitCompletion
            {
                Id = Guid.NewGuid(),
                HabitId = habit.Id,
                Date = today
            });

            await _context.SaveChangesAsync();

            habit = await _context.Habits.Include(h => h.Completions).FirstAsync(h => h.Id == id);
            return Ok(BuildHabitDto(habit, today));
        }

        [HttpDelete("{id}/complete")]
        public async Task<IActionResult> UndoComplete(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var habit = await _context.Habits
                .Include(h => h.Completions)
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (habit == null) return NotFound();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var todayEntry = habit.Completions.FirstOrDefault(c => c.Date == today);
            if (todayEntry != null)
            {
                _context.HabitCompletions.Remove(todayEntry);
                await _context.SaveChangesAsync();
            }

            habit = await _context.Habits.Include(h => h.Completions).FirstAsync(h => h.Id == id);
            return Ok(BuildHabitDto(habit, today));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHabit(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var habit = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);
            if (habit == null) return NotFound();

            _context.Habits.Remove(habit);
            await _context.SaveChangesAsync();

            return Ok(new { deleted = true });
        }

        [HttpGet("{id}/week")]
        public async Task<IActionResult> GetWeek(Guid id, [FromQuery] int offset = 0)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var habit = await _context.Habits
                .Include(h => h.Completions)
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (habit == null) return NotFound();
            if (offset > 0) return BadRequest(new { message = "Cannot view future weeks." });

            var today = DateOnly.FromDateTime(DateTime.Now);
            var completedDates = habit.Completions.Select(c => c.Date).ToHashSet();

            var days = new List<DayStatusDto>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays((offset * 7) - i);
                days.Add(new DayStatusDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    DayLabel = date.ToDateTime(TimeOnly.MinValue).ToString("ddd"),
                    Completed = completedDates.Contains(date)
                });
            }

            var rangeLabel = $"{DateOnly.Parse(days.First().Date):MMM d} – {DateOnly.Parse(days.Last().Date):MMM d}";

            return Ok(new WeekResponseDto { Days = days, RangeLabel = rangeLabel, Offset = offset });
        }

        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetDetails(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var habit = await _context.Habits
                .Include(h => h.Completions)
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if (habit == null) return NotFound();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var completedDates = habit.Completions.Select(c => c.Date).OrderBy(d => d).ToList();
            var completedSet = completedDates.ToHashSet();

            int currentStreak = 0;
            var cursor = today;
            if (!completedSet.Contains(cursor)) cursor = cursor.AddDays(-1);
            while (completedSet.Contains(cursor))
            {
                currentStreak++;
                cursor = cursor.AddDays(-1);
            }

            int longestStreak = 0;
            int running = 0;
            DateOnly? prev = null;
            foreach (var date in completedDates)
            {
                running = (prev.HasValue && date == prev.Value.AddDays(1)) ? running + 1 : 1;
                longestStreak = Math.Max(longestStreak, running);
                prev = date;
            }

            var createdDate = DateOnly.FromDateTime(habit.CreatedAt);
            int daysSinceCreation = Math.Max(1, today.DayNumber - createdDate.DayNumber + 1);
            double completionRate = Math.Round((double)completedDates.Count / daysSinceCreation * 100, 1);

            return Ok(new HabitDetailDto
            {
                Id = habit.Id,
                Name = habit.Name,
                Frequency = habit.Frequency,
                CreatedAt = createdDate.ToString("yyyy-MM-dd"),
                CurrentStreak = currentStreak,
                LongestStreak = longestStreak,
                TotalCompletions = completedDates.Count,
                CompletionRate = completionRate,
                CompletionDates = completedDates.Select(d => d.ToString("yyyy-MM-dd")).ToList()
            });
        }

        private static HabitDto BuildHabitDto(Habit habit, DateOnly today)
        {
            var completedDates = habit.Completions.Select(c => c.Date).ToHashSet();

            int streak = 0;
            var cursor = today;
            if (!completedDates.Contains(cursor)) cursor = cursor.AddDays(-1);
            while (completedDates.Contains(cursor))
            {
                streak++;
                cursor = cursor.AddDays(-1);
            }

            var last7 = new List<DayStatusDto>();
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                last7.Add(new DayStatusDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    DayLabel = date.ToDateTime(TimeOnly.MinValue).ToString("ddd"),
                    Completed = completedDates.Contains(date)
                });
            }

            return new HabitDto
            {
                Id = habit.Id,
                Name = habit.Name,
                Frequency = habit.Frequency,
                Streak = streak,
                CompletedToday = completedDates.Contains(today),
                Last7Days = last7
            };
        }
    }
}