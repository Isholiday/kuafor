namespace backend.Models;

public class Appointment {
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public int ServiceId { get; set; }
    public bool IsConfirmed { get; set; }
    public required Employee Employee { get; set; }
    public required Service Service { get; set; }
}
