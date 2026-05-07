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
}
