using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using detox.Data;
using detox.Models;
using detox.Services;
using detox.ViewModels;

namespace detox.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IStreakService _streakService;

    public HomeController(ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        IStreakService streakService)
    {
        _context = context;
        _userManager = userManager;
        _streakService = streakService;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.Identity!.IsAuthenticated) return View("Landing");

        var userId = _userManager.GetUserId(User)!;
        var today  = DateOnly.FromDateTime(DateTime.Today);

        var habits = await _context.Habits
            .Where(h => h.UserId == userId && !h.IsArchived)
            .Include(h => h.Logs)
            .OrderBy(h => h.CreatedAt)
            .ToListAsync();

        var cards = habits.Select(h =>
        {
            var streak    = _streakService.Compute(h, today);
            var todayLog  = h.Logs.FirstOrDefault(l => l.Date == today);
            var dueToday  = _streakService.IsDueToday(h, today);
            return new HabitCardViewModel
            {
                Habit = h, Streak = streak,
                TodayLog = todayLog,
                CompletedToday = todayLog?.IsCompleted == true,
                DueToday = dueToday
            };
        }).ToList();

        var due  = cards.Count(c => c.DueToday);
        var done = cards.Count(c => c.DueToday && c.CompletedToday);

        var vm = new DashboardViewModel
        {
            TodayHabits    = cards,
            DueToday       = due,
            CompletedToday = done
        };
        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
