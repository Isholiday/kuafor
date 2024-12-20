using System.ComponentModel.DataAnnotations;

namespace backend.Helpers;

public class RequiredListAttribute : ValidationAttribute {
    public RequiredListAttribute() : base("The {0} field must contain at least one item.") {
    }

    public override bool IsValid(object? value) {
        if (value == null)
            return false;

        if (value is List<string> list)
            return list.Count > 0 && list.Any(s => !string.IsNullOrWhiteSpace(s));

        return false;
    }
}
