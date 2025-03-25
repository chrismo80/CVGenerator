namespace CVGenerator.LaTeX;

public class Info
{
    public DateOnly Start { get; set; }

    public DateOnly End { get; set; }

    public string Text { get; set; } = "";

    public string Details { get; set; } = "";

    public Info()
    { }

    public Info(int startMonth, int startYear, int endMonth, int endYear, string text, string details = "") =>
        (Start, End, Text, Details) = (new(startYear, startMonth, 1), new(endYear, endMonth, 1), text, details);
}

public class Skill
{
    public string Text { get; set; }

    public int Percent { get; set; }

    public override string ToString() => $"{Text}-{Percent}";
}