using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Salon {
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string? Name { get; set; }

    [Required]
    [StringLength(200)]
    public string? Address { get; set; }

    [Required]
    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [Display(Name = "Opening Hours")]
    public TimeSpan OpeningTime { get; set; }

    [Required]
    [Display(Name = "Closing Hours")]
    public TimeSpan ClosingTime { get; set; }

    public List<Employee> Employees { get; set; } = [];

    public List<Service> Services { get; set; } = [];
}
