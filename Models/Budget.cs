using System.ComponentModel.DataAnnotations;

namespace C__Project.Models;

public class Budget
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a category.")]
    public int CategoryId { get; set; }

    // Navigation property
    public Category? Category { get; set; }

    [Required(ErrorMessage = "Budget amount is required.")]
    [Range(0.01, 10000000.00, ErrorMessage = "Budget amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Month { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
}
