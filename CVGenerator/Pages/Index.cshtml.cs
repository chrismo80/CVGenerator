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

    public string Details { get; set; }
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

    [BindProperty] public List<Skill> Skills { get; set; } = [];

    public string? Foto => ProfilePicture?.FileName;

    public string? Education => string.Join("\n", Educations.Select(e => e.FormatEducation()));

    public string? WorkExperience => string.Join("\n", WorkExperiences.Select(e => e.FormatWorkExperience()));

    public string? Skill => string.Join("\n\\newline\n", Skills.Select(e => e.ToString()));

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
        Name = HttpContext.Session.GetString("Name") ?? "John Doe";
        Email = HttpContext.Session.GetString("Email") ?? "test@gmail.com";
        Phone = HttpContext.Session.GetString("Phone") ?? "000023";

        // Load complex objects
        var educationJson = HttpContext.Session.GetString("Educations");

        if (educationJson != null)
            Educations = JsonSerializer.Deserialize<List<Info>>(educationJson);
        else
            Educations =
                [
                    new Info { Start = new(2000, 4, 1), End = new(2001, 6, 1), Details = "Abitur" },
                    new Info { Start = new(2002, 8, 1), End = new(2008, 1, 1), Details = "Diplomingenieur (FH)" }
                ];

        var workExperienceJson = HttpContext.Session.GetString("WorkExperiences");

        if (workExperienceJson != null)
            WorkExperiences = JsonSerializer.Deserialize<List<Info>>(workExperienceJson);
        else
            WorkExperiences =
                [
                    new Info { Start = new(2001, 7, 1), End = new(2002, 5, 1), Details = "Zivildienst" },
                    new Info { Start = new(2005, 1, 1), End = new(2006, 8, 1), Details = "IBM, Mainz" }
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