using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Service {
    public int Id { get; set; }

    public string? Name { get; set; }

    public decimal Price { get; set; }

    public TimeSpan Duration { get; set; }

    [Required(ErrorMessage = "Please select a salon")]
    [Display(Name = "Salon")]
    public int SalonId { get; set; }

    public Salon? Salon { get; set; }
}
