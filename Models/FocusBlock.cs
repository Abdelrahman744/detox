using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace detox.Models;

public class FocusBlock
{
    public int Id { get; set; }

    // Foreign key to AspNetUsers
    [Required]
    public string UserId { get; set; } = string.Empty;
    public IdentityUser? User { get; set; }

    [Required(ErrorMessage = "Activity Restricted is required.")]
    [Display(Name = "Activity Restricted")]
    public string ActivityRestricted { get; set; } = string.Empty;

    [Required(ErrorMessage = "Duration value is required.")]
    [Display(Name = "Duration")]
    [Range(0.1, 10000, ErrorMessage = "Duration must be greater than 0.")]
    public decimal DurationValue { get; set; }

    [Required(ErrorMessage = "Duration unit is required.")]
    [Display(Name = "Unit")]
    public string DurationUnit { get; set; } = "Hours";

    [Required]
    [Display(Name = "Success?")]
    public bool SuccessStatus { get; set; }

    [Required(ErrorMessage = "Date is required.")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    [Display(Name = "Reflection Notes")]
    [StringLength(500)]
    public string? Reflection { get; set; }
}
