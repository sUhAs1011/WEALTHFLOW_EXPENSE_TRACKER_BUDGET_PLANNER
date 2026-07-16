using System.ComponentModel.DataAnnotations;

namespace C__Project.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(50, ErrorMessage = "Category name cannot exceed 50 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select an icon.")]
    public string Icon { get; set; } = "📁"; // Default icon (emoji or class)

    [Required(ErrorMessage = "Please select a theme color.")]
    public string Color { get; set; } = "#cccccc"; // Default gray hex color

    // Relationships
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public Budget? Budget { get; set; }
}
