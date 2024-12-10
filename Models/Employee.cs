namespace backend.Models;

public class Employee {
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Specialization { get; set; }
    public List<string>? Skills { get; set; }
    public List<Availability>? AvailableHours { get; set; }
}
