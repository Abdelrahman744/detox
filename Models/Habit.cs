using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace detox.Models;

public class Habit
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public IdentityUser? User { get; set; }

    [Required(ErrorMessage = "Habit name is required.")]
    [StringLength(100)]
    [Display(Name = "Habit Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Description { get; set; }

    [Required]
    public string Category { get; set; } = "General";

    [Required]
    public string Icon { get; set; } = "⭕";

    [Required]
    public string Color { get; set; } = "#7c3aed";

    /// <summary>Daily | Weekly | Weekdays | Weekends | Custom</summary>
    [Required]
    public string Frequency { get; set; } = "Daily";

    /// <summary>Comma-separated for Custom: "Mon,Wed,Fri"</summary>
    public string? CustomDays { get; set; }

    public bool IsMeasurable { get; set; } = false;

    [Range(0.01, 100000)]
    public decimal? TargetValue { get; set; }

    [StringLength(30)]
    public string? TargetUnit { get; set; }

    public bool IsArchived { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<HabitLog> Logs { get; set; } = new List<HabitLog>();
}
