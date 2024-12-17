using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class User {
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string? Username { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string? Password { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    public bool IsAdmin { get; set; }
}

