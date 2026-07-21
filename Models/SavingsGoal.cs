using System.ComponentModel.DataAnnotations;

namespace C__Project.Models;

public class SavingsGoal
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Target title is required.")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Target amount is required.")]
    [Range(0.01, 10000000.00, ErrorMessage = "Target amount must be greater than zero.")]
    public decimal TargetAmount { get; set; }

    public decimal CurrentAmount { get; set; } = 0.00m;

    [Required(ErrorMessage = "Target completion date is required.")]
    public DateTime TargetDate { get; set; } = DateTime.Today.AddMonths(6);

    public string Icon { get; set; } = "🎯";

    public string Color { get; set; } = "#2ecc71";

    [StringLength(250, ErrorMessage = "Notes cannot exceed 250 characters.")]
    public string? Notes { get; set; }
}
