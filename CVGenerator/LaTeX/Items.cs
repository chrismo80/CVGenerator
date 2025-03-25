namespace CVGenerator.LaTeX;

public class Info
{
    public DateOnly Start { get; set; }

    public DateOnly End { get; set; }

    public string Text { get; set; } = "";

    public string Details { get; set; } = "";

    public string Content { get; set; } = "";

    public Info()
    { }

    public Info(int startMonth, int startYear, int endMonth, int endYear, string text, string details = "", string content = "") =>
        (Start, End, Text, Details, Content) = (new(startYear, startMonth, 1), new(endYear, endMonth, 1), text, details, content);
}

public class Skill
{
    public string Text { get; set; }

    public int Percent { get; set; }

    public int Range => Percent > 80 ? 1 : Percent > 40 ? 2 : 3;

    public Skill()
    { }

    public Skill(string text, int percent) =>
        (Text, Percent) = (text, percent);

    public override string ToString() => $"{Text}-{Percent}";
}