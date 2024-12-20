namespace backend.Helpers;
public static class ListExtensions {
    public static string ToCommaString(this List<string> list) {
        return list != null ? string.Join(",", list) : string.Empty;
    }
}
