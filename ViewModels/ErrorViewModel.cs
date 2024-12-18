namespace backend.ViewModels;

public class ErrorViewModel {
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
