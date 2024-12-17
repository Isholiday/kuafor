using System.ComponentModel.DataAnnotations;
namespace backend.Models;
public class RegisterViewModel {
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Password must be between 3 and 100 characters.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    [Display(Name = "Confirm Password")]
    public string? ConfirmPassword { get; set; }
}
