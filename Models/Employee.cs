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

    [RequiredList(ErrorMessage = "The Skills field is required.")]
    [Display(Name = "Skills (comma separated)")]
    public List<string> Skills { get; set; } = [];

    [Required]
    [JsonIgnore]
    public List<Availability> Availabilities { get; set; } = [];
}
