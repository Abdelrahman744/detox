using detox.Models;
using detox.Services;

namespace detox.ViewModels;

public class HabitCardViewModel
{
    public Habit Habit { get; set; } = null!;
    public StreakResult Streak { get; set; } = null!;
    public bool CompletedToday { get; set; }
    public bool DueToday { get; set; }
    public HabitLog? TodayLog { get; set; }
}

public class DashboardViewModel
{
    public List<HabitCardViewModel> TodayHabits { get; set; } = new();
    public int DueToday { get; set; }
    public int CompletedToday { get; set; }
    public double TodayRate => DueToday == 0 ? 0 : Math.Round((double)CompletedToday / DueToday * 100, 1);
}

public class AnalyticsViewModel
{
    public List<HabitCardViewModel> Habits { get; set; } = new();
    public int TotalHabits { get; set; }
    public string MostConsistent { get; set; } = "—";
    public string Nemesis { get; set; } = "—";
    public double OverallRate30 { get; set; }
}
