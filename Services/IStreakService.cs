using detox.Models;

namespace detox.Services;

public interface IStreakService
{
    StreakResult Compute(Habit habit, DateOnly today);
    bool IsDueToday(Habit habit, DateOnly today);
}

public class StreakResult
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public double CompletionRate7  { get; set; }
    public double CompletionRate30 { get; set; }
    public double CompletionRateAll { get; set; }
    public List<DateOnly> CompletedDates { get; set; } = new();
}
