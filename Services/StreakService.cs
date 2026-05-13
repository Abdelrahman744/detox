using detox.Models;

namespace detox.Services;

public class StreakService : IStreakService
{
    /// <summary>
    /// Returns whether the habit is scheduled for the given day.
    /// </summary>
    public bool IsDueToday(Habit habit, DateOnly today)
    {
        return habit.Frequency switch
        {
            "Daily"    => true,
            "Weekdays" => today.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday,
            "Weekends" => today.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday,
            "Weekly"   => true,   // owner decides which day; we just show it
            "Custom"   => IsCustomDay(habit.CustomDays, today),
            _          => true
        };
    }

    /// <summary>
    /// Computes streaks and completion rates from already-loaded Habit.Logs.
    /// Call with habit.Logs populated (Include in EF query).
    /// </summary>
    public StreakResult Compute(Habit habit, DateOnly today)
    {
        var completed = habit.Logs
            .Where(l => l.IsCompleted)
            .Select(l => l.Date)
            .ToHashSet();

        var result = new StreakResult { CompletedDates = completed.ToList() };

        // --- Current streak ---
        int current = 0;
        var day = today;
        while (true)
        {
            if (!IsDueToday(habit, day)) { day = day.AddDays(-1); continue; }
            if (!completed.Contains(day)) break;
            current++;
            day = day.AddDays(-1);
        }
        result.CurrentStreak = current;

        // --- Longest streak ---
        if (completed.Count == 0)
        {
            result.LongestStreak = 0;
        }
        else
        {
            var sortedDates = completed.OrderBy(d => d).ToList();
            int longest = 1, run = 1;
            for (int i = 1; i < sortedDates.Count; i++)
            {
                // Walk day-by-day between the two dates, skipping non-due days
                var prev = sortedDates[i - 1];
                var curr = sortedDates[i];
                var expected = NextDueDay(habit, prev);
                if (expected == curr) { run++; longest = Math.Max(longest, run); }
                else                  { run = 1; }
            }
            result.LongestStreak = longest;
        }

        // --- Completion rates ---
        result.CompletionRate7  = Rate(habit, completed, today, 7);
        result.CompletionRate30 = Rate(habit, completed, today, 30);
        result.CompletionRateAll = completed.Count == 0 ? 0 :
            (double)completed.Count / DueDaysInRange(habit,
                DateOnly.FromDateTime(habit.CreatedAt.Date), today) * 100;

        return result;
    }

    // --- helpers ---

    private static bool IsCustomDay(string? customDays, DateOnly date)
    {
        if (string.IsNullOrWhiteSpace(customDays)) return true;
        var name = date.DayOfWeek.ToString()[..3]; // "Mon", "Tue" …
        return customDays.Split(',').Any(d => d.Trim().Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private DateOnly NextDueDay(Habit habit, DateOnly from)
    {
        var d = from.AddDays(1);
        for (int i = 0; i < 14; i++, d = d.AddDays(1))
            if (IsDueToday(habit, d)) return d;
        return d;
    }

    private double Rate(Habit habit, HashSet<DateOnly> completed, DateOnly today, int days)
    {
        int due  = DueDaysInRange(habit, today.AddDays(-(days - 1)), today);
        if (due == 0) return 0;
        int done = Enumerable.Range(0, days)
            .Select(i => today.AddDays(-i))
            .Count(d => IsDueToday(habit, d) && completed.Contains(d));
        return Math.Round((double)done / due * 100, 1);
    }

    private int DueDaysInRange(Habit habit, DateOnly from, DateOnly to)
    {
        int count = 0;
        for (var d = from; d <= to; d = d.AddDays(1))
            if (IsDueToday(habit, d)) count++;
        return count;
    }
}
