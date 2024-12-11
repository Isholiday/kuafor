using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Availability {
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public required Employee Employee { get; set; }
}
