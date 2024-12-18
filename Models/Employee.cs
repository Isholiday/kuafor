using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Employee {
    public int Id { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public string? Specialization { get; set; }

    [Required]
    public List<string>? Skills { get; set; }

    [Required]
    public List<Availability>? Availabilities { get; set; }
}
