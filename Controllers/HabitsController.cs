using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using detox.Data;
using detox.Models;
using detox.Services;
using detox.ViewModels;

namespace detox.Controllers;

[Authorize]
public class HabitsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IStreakService _streakService;

    public HabitsController(ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        IStreakService streakService)
    {
        _context = context;
        _userManager = userManager;
        _streakService = streakService;
    }

    // GET: /Habits
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var today  = DateOnly.FromDateTime(DateTime.Today);

        var habits = await _context.Habits
            .Where(h => h.UserId == userId && !h.IsArchived)
            .Include(h => h.Logs)
            .OrderBy(h => h.CreatedAt)
            .ToListAsync();

        var cards = habits.Select(h => BuildCard(h, today)).ToList();
        return View(cards);
    }

    // GET: /Habits/Create
    public IActionResult Create() => View(new Habit());

    // POST: /Habits/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Habit habit)
    {
        habit.UserId = _userManager.GetUserId(User)!;
        ModelState.Remove("UserId"); ModelState.Remove("User"); ModelState.Remove("Logs");

        if (!ModelState.IsValid) return View(habit);

        _context.Habits.Add(habit);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Habits/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var habit  = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);
        if (habit == null) return NotFound();
        return View(habit);
    }

    // POST: /Habits/Edit/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Habit habit)
    {
        if (id != habit.Id) return NotFound();
        var userId = _userManager.GetUserId(User)!;
        var existing = await _context.Habits.AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);
        if (existing == null) return NotFound();

        habit.UserId    = userId;
        habit.CreatedAt = existing.CreatedAt;
        ModelState.Remove("UserId"); ModelState.Remove("User"); ModelState.Remove("Logs");

        if (!ModelState.IsValid) return View(habit);

        _context.Update(habit);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST: /Habits/Archive/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var habit  = await _context.Habits.FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);
        if (habit != null) { habit.IsArchived = true; await _context.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    // POST: /Habits/CheckIn/5  (AJAX toggle)
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(int id, decimal? value, string? notes)
    {
        var userId = _userManager.GetUserId(User)!;
        var today  = DateOnly.FromDateTime(DateTime.Today);

        var habit = await _context.Habits
            .Include(h => h.Logs)
            .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);
        if (habit == null) return NotFound();

        var log = habit.Logs.FirstOrDefault(l => l.Date == today);
        if (log == null)
        {
            log = new HabitLog { HabitId = id, UserId = userId, Date = today, IsCompleted = true, Value = value, Notes = notes, LoggedAt = DateTime.UtcNow };
            _context.HabitLogs.Add(log);
        }
        else
        {
            log.IsCompleted = !log.IsCompleted;
            if (log.IsCompleted) { log.Value = value; log.Notes = notes; log.LoggedAt = DateTime.UtcNow; }
        }
        await _context.SaveChangesAsync();

        // Re-load logs for streak calculation
        await _context.Entry(habit).Collection(h => h.Logs).LoadAsync();
        var streak = _streakService.Compute(habit, today);

        return Json(new { completed = log.IsCompleted, currentStreak = streak.CurrentStreak });
    }

    // GET: /Habits/Analytics
    public async Task<IActionResult> Analytics()
    {
        var userId = _userManager.GetUserId(User)!;
        var today  = DateOnly.FromDateTime(DateTime.Today);

        var habits = await _context.Habits
            .Where(h => h.UserId == userId && !h.IsArchived)
            .Include(h => h.Logs)
            .OrderBy(h => h.CreatedAt)
            .ToListAsync();

        var cards = habits.Select(h => BuildCard(h, today)).ToList();

        var vm = new AnalyticsViewModel
        {
            Habits       = cards,
            TotalHabits  = habits.Count,
            MostConsistent = cards.OrderByDescending(c => c.Streak.CurrentStreak).FirstOrDefault()?.Habit.Name ?? "—",
            Nemesis        = cards.OrderBy(c => c.Streak.CompletionRate30).FirstOrDefault()?.Habit.Name ?? "—",
            OverallRate30  = cards.Count == 0 ? 0 : Math.Round(cards.Average(c => c.Streak.CompletionRate30), 1)
        };
        return View(vm);
    }

    // --- helpers ---
    private HabitCardViewModel BuildCard(Habit habit, DateOnly today)
    {
        var streak    = _streakService.Compute(habit, today);
        var todayLog  = habit.Logs.FirstOrDefault(l => l.Date == today);
        var dueToday  = _streakService.IsDueToday(habit, today);
        return new HabitCardViewModel
        {
            Habit = habit,
            Streak = streak,
            TodayLog = todayLog,
            CompletedToday = todayLog?.IsCompleted == true,
            DueToday = dueToday
        };
    }
}
