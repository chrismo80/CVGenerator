using System.Reflection;
using System.Text.Json;

namespace CVGenerator.LaTeX;

public static class DataHandler
{
    public static Dictionary<string, object?> GetFields<TProperty>(this object page) => page.GetType()
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(prop => prop.PropertyType == typeof(TProperty))
        .ToDictionary(prop => prop.Name, prop => prop.GetValue(page));

    public static Dictionary<string, TProperty?> GetBindedFields<TProperty, TAttribute>(this object page) => page.GetType()
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(prop => prop.PropertyType == typeof(TProperty))
        .Where(prop => Attribute.IsDefined(prop, typeof(TAttribute)))
        .ToDictionary(prop => prop.Name, prop => (TProperty?)prop.GetValue(page));

    public static IEnumerable<PropertyInfo> GetBindedProperties<TProperty, TAttribute>(this object page) => page.GetType()
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(prop => prop.PropertyType == typeof(TProperty))
        .Where(prop => Attribute.IsDefined(prop, typeof(TAttribute)));

    public static void SetProperty(this PropertyInfo prop, object page, object? value)
    {
        if (value != null)
            prop.SetValue(page, value);
    }

    public static T? LoadFromJson<T>(this string text) => JsonSerializer.Deserialize<T>(text);

    public static string Format(this Info info, string type) => "\\" + type +
        "{" + info.Start.Month + "/" + info.Start.Year + "}" +
        "{" + info.End.Month + "/" + info.End.Year + "}" +
        "{" + info.Text + "}{" + info.Details + "}{" + info.Content + "}";

    public static string FormatBarChart(this Skill skill) => "\\baritem{100}" +
        "{" + skill.Text + "}" +
        "{" + (skill.Level == 1 ? skill.Percent : "0") + "}" +
        "{" + (skill.Level == 2 ? skill.Percent : "0") + "}" +
        "{" + (skill.Level == 3 ? skill.Percent : "0") + "}";

    public static string FormatPieChart(this Skill skill) => "\\slice" +
        "{" + skill.Percent + "}" +
        "{" + skill.Text + "}" +
        "{color" + skill.Level + "}";

    public static string Format(this List<Info> infos, string prefix) =>
        string.Join("\n", infos.Select(info => info.Format(prefix)));

    public static string FormatBarChart(this List<Skill> skills) =>
        string.Join("\n", skills.Select(skill => skill.FormatBarChart()));

    public static string FormatPieChart(this List<Skill> skills) =>
        string.Join("\n", skills.Select(skill => skill.FormatPieChart()));

    public static string Sanitize(this string input)
    {
        foreach (var c in "#") // "#$%&_{}"
            input = input.Replace(c.ToString(), @$"\{c}");

        return input.Replace("\r\n\r\n", "\r\n\\newline\r\n\r\n");

        return input
            .Replace(@"\", @"\textbackslash{}")
            .Replace("~", @"\textasciitilde{}")
            .Replace("^", @"\textasciicircum{}");
    }
}