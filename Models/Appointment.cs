namespace backend.Models;

public class Appointment {
    public int Id { get; set; }

    public int UserId { get; set; }

    public int EmployeeId { get; set; }

    public int SalonId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public int ServiceId { get; set; }

    public bool IsConfirmed { get; set; }

    public User? User { get; set; }

    public Employee? Employee { get; set; }

    public Service? Service { get; set; }

    public Salon? Salon { get; set; }
}
