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
    public string Text { get; set; } = "";

    public int Percent { get; set; }

    public int Level { get; set; }

    public Skill()
    { }

    public Skill(string text, int percent, int level = 1) =>
        (Text, Percent, Level) = (text, percent, level);
}