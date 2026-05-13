using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace detox.Models;

public class HabitLog
{
    public int Id { get; set; }

    [Required]
    public int HabitId { get; set; }
    public Habit? Habit { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public IdentityUser? User { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    public bool IsCompleted { get; set; } = false;

    /// <summary>For measurable habits: the actual value achieved.</summary>
    public decimal? Value { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}
