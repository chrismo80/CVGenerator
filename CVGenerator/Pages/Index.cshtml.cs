using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using System.Reflection;
using System.Text.Json;

using CVGenerator.LaTeX;

namespace CVGenerator.Pages;

public class Info
{
    public DateOnly Start { get; set; }
    public DateOnly End { get; set; }
    public string Text { get; set; } = "";
    public string Details { get; set; } = "";

    public Info(){}

    public Info(int startMonth, int startYear, int endMonth, int endYear, string text, string details = "") =>
        (Start, End, Text, Details) = (new(startYear, startMonth, 1), new(endYear, endMonth, 1), text, details);
}

public class Skill
{
    public string Text { get; set; }
    public int Percent { get; set; }

    public override string ToString() => $"{Text}-{Percent}";
}

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    [BindProperty] public IFormFile? ProfilePicture { get; set; }

    [BindProperty] public string Name { get; set; }

    [BindProperty] public string Email { get; set; }

    [BindProperty] public string Phone { get; set; }

    [BindProperty] public List<Info> Educations { get; set; } = [];

    [BindProperty] public List<Info> WorkExperiences { get; set; } = [];

    [BindProperty] public List<Info> Projects { get; set; } = [];

    [BindProperty] public List<Skill> Skills { get; set; } = [];

    public string? Foto => ProfilePicture?.FileName;

    public string? Education => string.Join("\n", Educations.Select(e => e.FormatInfo("cveventleft")));

    public string? WorkExperience => string.Join("\n", WorkExperiences.Select(e => e.FormatInfo("cveventright")));

    public string? Project => string.Join("\n", Projects.Select(e => e.FormatInfo("cveventproject")));

    public string? Skill => string.Join("\n\\newline\n", Skills.Select(e => e.ToString()));

    public string? MinYear => (Educations.Concat(WorkExperiences).Concat(Projects).Min(e => e.Start.Year) + 1).ToString();

    public string? MaxYear => (Educations.Concat(WorkExperiences).Concat(Projects).Max(e => e.End.Year) + 0).ToString();

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> OnPostGenerateAsync()
    {
        OnSet();

        Directory.CreateDirectory("temp");

        await SaveProfilePicture();

        var pdfBytes = await GetFields()!.GeneratePdf();

        return File(pdfBytes, "application/pdf", Name + ".pdf");
    }

    public void OnSet()
    {
        // Save data to session before generating the PDF
        HttpContext.Session.SetString("Name", Name);
        HttpContext.Session.SetString("Email", Email);
        HttpContext.Session.SetString("Phone", Phone);

        // You can also store complex objects like lists using JSON serialization
        HttpContext.Session.SetString("Educations", JsonSerializer.Serialize(Educations));
        HttpContext.Session.SetString("WorkExperiences", JsonSerializer.Serialize(WorkExperiences));
        HttpContext.Session.SetString("Skills", JsonSerializer.Serialize(Skills));
    }

    public void OnGet()
    {
        // Load session data on page visit
        Name = HttpContext.Session.GetString("Name") ?? "Max Mustermann";
        Email = HttpContext.Session.GetString("Email") ?? "max@gmail.de";
        Phone = HttpContext.Session.GetString("Phone") ?? "0133 / 123 456 78";

        // Load complex objects
        var educationJson = HttpContext.Session.GetString("Educations");

        if (educationJson != null)
            Educations = JsonSerializer.Deserialize<List<Info>>(educationJson);
        else
            Educations =
                [
                    new Info(4, 2000, 6, 2001, "Abitur", "Bad Sobernheim"),
                    new Info(8, 2002, 1, 2008, "Diplomingenieur (FH)", "Elektrotechnik\\\\FH Bingen"),
                ];

        var workExperienceJson = HttpContext.Session.GetString("WorkExperiences");

        if (workExperienceJson != null)
            WorkExperiences = JsonSerializer.Deserialize<List<Info>>(workExperienceJson);
        else
            WorkExperiences =
                [
                    new Info(7, 2001, 5, 2002, "Zivildienst", "(Tagesförderstätte)"),
                    new Info(1, 2005, 8, 2006, "IBM, Mainz", "(Werkstudent)"),
                    new Info(11, 2006, 4, 2007, "SCHOTT, Mainz", "(Praktikant)"),
                    new Info(8, 2007, 1, 2008, "ESCO, Mainz", "(Diplomand)"),
                    new Info(2, 2008, 6, 2025, "ESCO, Mainz"),
                    new Info(7, 2025, 6, 2026, "Traumfirma, Frankfurt"),
                ];

        var projectJson = HttpContext.Session.GetString("Projects");

        if (projectJson != null)
            Projects = JsonSerializer.Deserialize<List<Info>>(projectJson);
        else
            Projects =
            [
                new Info(4, 2002, 4, 2002, "Betreuung von Menschen mit Behinderung\\\\in einer Tagesförderstätte"),
                new Info(6, 2005, 2, 2006, "Lotus Notes Datenbankentwicklung"),
                new Info(1, 2009, 1, 2011, "Integration von Mess- und Prüftechnik in Prozessautomation (Hardwareansteuerung, Implementierung von Mess-Algorithmen in C\\#)"),
                new Info(1, 2012, 1, 2014, "Erstellen von projektspezifischen .NET Framework Desktop-Applikationen im Sondermaschinenbau (Front- und Back-End)"),
                new Info(1, 2015, 1, 2017, "Pflege des hauseigenen projektunabhängigen C\\#-Frameworks zur Erstellung von Maschinensteuerungen und -visualisierungen (Mocks, Unit-Tests)"),
                new Info(1, 2018, 1, 2019, "Anforderungsanalysen (Pflichten- und Lastenhefte)"),
                new Info(1, 2020, 1, 2022, "Integration unterschiedlichster Technologien in Projekt-Applikationen (TwinCat, Halcon, OPC-UA, REST, gRPC, GenICam, 3rd party libraries)"),
                new Info(1, 2023, 1, 2024, "Design von Datenrückverfolgbarkeitssystemen mittels Entity Framework inkl. Berichtswesen (SQL, SSRS)"),
            ];

        var skillsJson = HttpContext.Session.GetString("Skills");

        if (skillsJson != null)
            Skills = JsonSerializer.Deserialize<List<Skill>>(skillsJson);
        else
            Skills =
            [
                new Skill() { Text = "German", Percent = 60},
                new Skill() { Text = "English", Percent = 40},
            ];
    }

    private async Task SaveProfilePicture()
    {
        if (ProfilePicture == null)
            return;

        await using var fileStream = new FileStream(Path.Combine("temp", ProfilePicture.FileName), FileMode.Create);
        await ProfilePicture.CopyToAsync(fileStream);
    }

    private Dictionary<string, object?> GetFields() => GetType()
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(prop => prop.PropertyType == typeof(string))
        .ToDictionary(prop => prop.Name, prop => prop.GetValue(this));
}