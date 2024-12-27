using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using backend.Helpers;

namespace backend.Models;

public class Employee {
    public int Id { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public string? Specialization { get; set; }

    [RequiredList]
    public List<string> Skills { get; set; } = [];

    [Required]
    [Display(Name = "Salon")]
    public int SalonId { get; set; }

    [JsonIgnore]
    public Salon? Salon { get; set; }

    [Required]
    [JsonIgnore]
    public List<Availability> Availabilities { get; set; } = [];
}
