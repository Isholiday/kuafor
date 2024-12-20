using System.Text.Json.Serialization;

namespace backend.Models;

public class Availability {
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    [JsonIgnore]
    public Employee? Employee { get; set; }
}
