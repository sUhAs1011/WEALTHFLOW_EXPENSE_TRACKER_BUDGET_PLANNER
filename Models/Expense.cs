using System.ComponentModel.DataAnnotations;

namespace C__Project.Models;

public class Expense
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, 10000000.00, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Date is required.")]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Please select a category.")]
    public int CategoryId { get; set; }

    // Navigation property
    public Category? Category { get; set; }

    [StringLength(250, ErrorMessage = "Notes cannot exceed 250 characters.")]
    public string? Notes { get; set; }

    public bool IsRecurring { get; set; }
}
