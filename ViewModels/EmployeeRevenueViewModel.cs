namespace backend.ViewModels;
public class EmployeeRevenueViewModel {
    public string EmployeeName { get; set; } = string.Empty;
    public int AppointmentCount { get; set; }
    public double TotalHours { get; set; }
    public decimal Revenue { get; set; }
}
