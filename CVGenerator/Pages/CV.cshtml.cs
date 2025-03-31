using Microsoft.AspNetCore.Mvc;

using CVGenerator.LaTeX;

namespace CVGenerator.Pages;

public class CVModel(ILogger<CVModel> logger) : DocumentPageModel
{
    private readonly ILogger<CVModel> _logger = logger;

    public override string DataFolder => "CV";

    public override string FileName => Name + " - " + NewRole;

    [BindProperty] public string Company { get; set; } = "";

    [BindProperty] public string Address { get; set; } = "";

    [BindProperty] public string City { get; set; } = "";

    [BindProperty] public string Recruiter { get; set; } = "";

    [BindProperty] public string NewRole { get; set; } = "";

    [BindProperty] public string Product { get; set; } = "";

    [BindProperty] public string Name { get; set; } = "";

    [BindProperty] public string Location { get; set; } = "";

    [BindProperty] public string Email { get; set; } = "";

    [BindProperty] public string Link { get; set; } = "";

    [BindProperty] public string Phone { get; set; } = "";

    [BindProperty] public string Role { get; set; } = "";

    [BindProperty] public string Degree { get; set; } = "";

    [BindProperty] public IFormFile? ProfilePicture { get; set; }

    [BindProperty] public IFormFile? Signature { get; set; }

    [BindProperty] public string Summary { get; set; } = "";

    [BindProperty] public string Letter { get; set; } = "";

    [BindProperty] public string Postskriptum { get; set; } = "";

    [BindProperty] public List<Info> Educations { get; set; } = [];

    [BindProperty] public List<Info> WorkExperiences { get; set; } = [];

    [BindProperty] public List<Info> Projects { get; set; } = [];

    [BindProperty] public List<Skill> Skills { get; set; } = [];

    [BindProperty] public List<Skill> Languages { get; set; } = [];

    [BindProperty] public List<Skill> Versionings { get; set; } = [];

    public string? Foto => ProfilePicture?.FileName;

    public string? Education => Educations.Format("cveventleft");

    public string? WorkExperience => WorkExperiences.Format("cveventright");

    public string? Project => Projects.Format("cveventproject");

    public string? Skill => Skills.FormatBarChart();

    public string? Language => Languages.FormatPieChart();

    public string? Versioning => Versionings.FormatPieChart();

    public string? MinYear => (Timeline.Any() ? Timeline.Min(e => e.Start.Year) : 2003 + 1).ToString();

    public string? MaxYear => (Timeline.Any() ? Timeline.Max(e => e.End.Year) : 2012 + 0).ToString();

    public List<Info> Timeline => Educations.Concat(WorkExperiences).Concat(Projects).ToList();

    protected override async Task OnGenerate()
    {
        await ProfilePicture.SaveAsyncToTempFolder("foto.png");
        await Signature.SaveAsyncToTempFolder("signature.png");
    }
}