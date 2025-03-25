using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using System.Text.Json;

using CVGenerator.LaTeX;

namespace CVGenerator.Pages;

public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    private readonly ILogger<IndexModel> _logger = logger;

    [BindProperty] public IFormFile? ProfilePicture { get; set; }

    [BindProperty] public string Name { get; set; } = "Max Mustermann";

    [BindProperty] public string Email { get; set; } = "max@gmail.de";

    [BindProperty] public string Phone { get; set; } = "0133 / 123 456 78";

    [BindProperty]
    public List<Info> Educations { get; set; } =
        [
            new Info(4, 2000, 6, 2001, "Abitur", "Bad Sobernheim"),
            new Info(8, 2002, 1, 2008, "Diplomingenieur (FH)", "Elektrotechnik\\\\FH Bingen"),
        ];

    [BindProperty]
    public List<Info> WorkExperiences { get; set; } =
        [
            new Info(7, 2001, 5, 2002, "Zivildienst", "(Tagesförderstätte)"),
            new Info(1, 2005, 8, 2006, "IBM, Mainz", "(Werkstudent)"),
            new Info(11, 2006, 4, 2007, "SCHOTT, Mainz", "(Praktikant)"),
            new Info(8, 2007, 1, 2008, "ESCO, Mainz", "(Diplomand)"),
            new Info(2, 2008, 6, 2025, "ESCO, Mainz"),
            new Info(7, 2025, 6, 2026, "Traumfirma, Frankfurt"),
        ];

    [BindProperty]
    public List<Info> Projects { get; set; } =
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

    [BindProperty]
    public List<Skill> Skills { get; set; } =
        [
            new Skill() { Text = "German", Percent = 60},
            new Skill() { Text = "English", Percent = 40},
        ];

    public string? Foto => ProfilePicture?.FileName;

    public string? Education => Educations.Format("cveventleft");

    public string? WorkExperience => WorkExperiences.Format("cveventright");

    public string? Project => Projects.Format("cveventproject");

    public string? Skill => string.Join("\n\\newline\n", Skills.Select(e => e.ToString()));

    public string? MinYear => (Educations.Concat(WorkExperiences).Concat(Projects).Min(e => e.Start.Year) + 1).ToString();

    public string? MaxYear => (Educations.Concat(WorkExperiences).Concat(Projects).Max(e => e.End.Year) + 0).ToString();

    public async Task<IActionResult> OnPostGenerateAsync()
    {
        OnSet();

        Directory.CreateDirectory("temp");

        await SaveProfilePicture();

        var pdfBytes = await this.GetFields<string>()!.GeneratePdf();

        return File(pdfBytes, "application/pdf", Name + ".pdf");
    }

    public void OnSet()
    {
        foreach (var (name, value) in this.GetBindedFields<string, BindPropertyAttribute>())
            HttpContext.Session.SetString(name, value);

        foreach (var (name, value) in this.GetBindedFields<List<Info>, BindPropertyAttribute>().Where(kvp => kvp!.Value!.Any()))
            HttpContext.Session.SetString(name, JsonSerializer.Serialize(value));

        foreach (var (name, value) in this.GetBindedFields<List<Skill>, BindPropertyAttribute>().Where(kvp => kvp!.Value!.Any()))
            HttpContext.Session.SetString(name, JsonSerializer.Serialize(value));
    }

    public void OnGet()
    {
        foreach (var prop in this.GetBindedProperties<string, BindPropertyAttribute>())
            prop.SetProperty(this, HttpContext.Session.GetString(prop.Name));

        foreach (var prop in this.GetBindedProperties<List<Info>, BindPropertyAttribute>())
            prop.SetProperty(this, HttpContext.Session.GetString(prop.Name)?.LoadFromJson<List<Info>>());

        foreach (var prop in this.GetBindedProperties<List<Skill>, BindPropertyAttribute>())
            prop.SetProperty(this, HttpContext.Session.GetString(prop.Name)?.LoadFromJson<List<Skill>>());
    }

    private async Task SaveProfilePicture()
    {
        if (ProfilePicture == null)
            return;

        await using var fileStream = new FileStream(Path.Combine("temp", ProfilePicture.FileName), FileMode.Create);
        await ProfilePicture.CopyToAsync(fileStream);
    }
}