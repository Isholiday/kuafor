using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Appointment {
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required(ErrorMessage = "Please select an employee")]
    [Display(Name = "Employee")]
    public int EmployeeId { get; set; }

    [Required(ErrorMessage = "Please select a salon")]
    [Display(Name = "Salon")]
    public int SalonId { get; set; }

    public DateTime AppointmentDate { get; set; }

    [Required(ErrorMessage = "Please select a service")]
    [Display(Name = "Service")]
    public int ServiceId { get; set; }

    public bool IsConfirmed { get; set; }

    public User? User { get; set; }

    public Employee? Employee { get; set; }

    public Service? Service { get; set; }

    public Salon? Salon { get; set; }
}
