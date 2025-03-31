using System.Reflection;
using System.Text.RegularExpressions;

namespace CVGenerator.LaTeX;

public static partial class DataHandler
{
    public static Dictionary<string, object?> GetFields<TProperty>(this object page) => page.GetType()
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(prop => prop.PropertyType == typeof(TProperty))
        .ToDictionary(prop => prop.Name, prop => prop.GetValue(page));

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

        return input
            .Replace("\r\n\r\n", "\r\n\\newline\r\n\r\n")
            .ParseLinks();

        return input
            .Replace(@"\", @"\textbackslash{}")
            .Replace("~", @"\textasciitilde{}")
            .Replace("^", @"\textasciicircum{}");
    }

    [GeneratedRegex(@"(?<PRE>.*)(?<REPLACE>.*\[(?<URL>.*)\:(?<TEXT>.*)\])(?<POST>.*)")]
    private static partial Regex Link();

    private static string ParseLinks(this string text)
    {
        var match = Link().Match(text);

        while (match.Success)
        {
            text = text.Replace(match.Groups["REPLACE"].Value,
                $@"\href{{{match.Groups["URL"].Value}}}{{\textbf{{\ul{{{match.Groups["TEXT"].Value}}}}}}}");

            match = Link().Match(text);
        }

        return text;
    }
}