using System.ComponentModel.DataAnnotations;
namespace backend.ViewModels;

public class HairAdvisorViewModel {
    [Required(ErrorMessage = "Please select a photo")]
    [Display(Name = "Your Photo")]
    [DataType(DataType.Upload)]
    public IFormFile? Photo { get; set; }

    [Display(Name = "AI Recommendations")]
    public string? RecommendationTextResult { get; set; }

    public List<string>? RecommendationImageResults { get; set; }
}
