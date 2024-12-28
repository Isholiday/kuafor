namespace backend.Helpers;
public static class ListExtensions {
    public static string ToCommaString(this List<string> list) => string.Join(",", list);
}
