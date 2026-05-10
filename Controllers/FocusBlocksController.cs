using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using detox.Data;
using detox.Models;

namespace detox.Controllers;

[Authorize]
public class FocusBlocksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public FocusBlocksController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: FocusBlocks (The Archive - Read & Delete view)
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var focusBlocks = await _context.FocusBlocks
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.Date)
            .ToListAsync();
        return View(focusBlocks);
    }

    // GET: FocusBlocks/Create (The Terminal - Create)
    public IActionResult Create()
    {
        return View();
    }

    // POST: FocusBlocks/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ActivityRestricted,DurationValue,DurationUnit,SuccessStatus,Date,Reflection")] FocusBlock focusBlock)
    {
        // Assign the current user's ID before saving
        focusBlock.UserId = _userManager.GetUserId(User)!;

        // Remove UserId and User from validation since we set them server-side
        ModelState.Remove("UserId");
        ModelState.Remove("User");

        if (ModelState.IsValid)
        {
            _context.Add(focusBlock);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(focusBlock);
    }

    // GET: FocusBlocks/Edit/5 (The Terminal - Update)
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        var focusBlock = await _context.FocusBlocks
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

        if (focusBlock == null) return NotFound();

        return View(focusBlock);
    }

    // POST: FocusBlocks/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ActivityRestricted,DurationValue,DurationUnit,SuccessStatus,Date,Reflection")] FocusBlock focusBlock)
    {
        if (id != focusBlock.Id) return NotFound();

        // Ensure the block belongs to the current user
        var userId = _userManager.GetUserId(User);
        var existingBlock = await _context.FocusBlocks
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

        if (existingBlock == null) return NotFound();

        focusBlock.UserId = userId!;
        ModelState.Remove("UserId");
        ModelState.Remove("User");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(focusBlock);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FocusBlockExists(focusBlock.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(focusBlock);
    }

    // POST: FocusBlocks/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User);
        var focusBlock = await _context.FocusBlocks
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

        if (focusBlock != null)
        {
            _context.FocusBlocks.Remove(focusBlock);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool FocusBlockExists(int id)
    {
        return _context.FocusBlocks.Any(e => e.Id == id);
    }






    // GET: FocusBlocks/Analytics
public async Task<IActionResult> Analytics()
{
    var userId = _userManager.GetUserId(User);
    
    // Fetch only this user's data
    var blocks = await _context.FocusBlocks
        .Where(f => f.UserId == userId)
        .ToListAsync();

    // 1. Calculate Win Rate
    var totalBlocks = blocks.Count;
    var successfulBlocks = blocks.Count(b => b.SuccessStatus);
    var winRate = totalBlocks > 0 ? Math.Round((double)successfulBlocks / totalBlocks * 100, 1) : 0;

    // 2. Calculate Total Hours (Normalizing the different Duration Units)
    decimal totalHours = 0;
    foreach (var b in blocks)
    {
        if (b.DurationUnit == "Hours") totalHours += b.DurationValue;
        else if (b.DurationUnit == "Days") totalHours += b.DurationValue * 24;
        else if (b.DurationUnit == "Months") totalHours += b.DurationValue * 720;
        else if (b.DurationUnit == "Seconds") totalHours += b.DurationValue / 3600;
    }

    // 3. Find the "Nemesis" (The activity failed most often)
    var nemesis = blocks.Where(b => !b.SuccessStatus)
                        .GroupBy(b => b.ActivityRestricted)
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .FirstOrDefault() ?? "None. Unbreakable.";

    // Pass data to the view using ViewBag
    ViewBag.TotalBlocks = totalBlocks;
    ViewBag.WinRate = winRate;
    ViewBag.TotalHours = Math.Round(totalHours, 1);
    ViewBag.Nemesis = nemesis;

    return View();
}
}
