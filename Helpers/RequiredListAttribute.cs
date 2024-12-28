using System.ComponentModel.DataAnnotations;

namespace backend.Helpers;

public class RequiredListAttribute() : ValidationAttribute("The {0} field must contain at least one item.") {
    public override bool IsValid(object? value) {
        return value switch {
            null => false,
            List<string> list => list.Count > 0 && list.Any(s => !string.IsNullOrWhiteSpace(s)),
            _ => false
        };
    }
}
