using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using System.Text;
using System.Text.Json;
using System.Reflection;

using CVGenerator.LaTeX;

namespace CVGenerator.Pages;

public abstract class DocumentPageModel : PageModel
{
    static readonly JsonSerializerOptions options = new() { WriteIndented = true };

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CVModel> _logger;
    private readonly IEnumerable<PropertyInfo> _props;

    public abstract string DataFolder { get; }

    public abstract string FileName { get; }

    [BindProperty] public IFormFile? UploadedFile { get; set; }

    protected DocumentPageModel(ILogger<CVModel> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _props = GetProperties();
    }

    public IActionResult OnPostExport() =>
        File(Encoding.UTF8.GetBytes(Serialize()), "application/json", GetType().Name + ".json");

    public async Task<IActionResult> OnPostAsync() => Request.Form["handler"].ToString() switch
    {
        "Generate" => await OnPostGenerateAsync(),
        "LoadExample" => await OnPostLoadExampleAsync(),
        "Import" => await OnPostImportAsync(),
        "Export" => OnPostExport(),
        _ => Page()
    };

    public async Task<IActionResult> OnPostGenerateAsync()
    {
        OnSet();

        var sessionId = _httpContextAccessor?.HttpContext?.Session.Id ?? "Hmmmm";

        _logger.LogInformation("Session: " + sessionId + " for folder: " + Path.Combine(Directory.GetCurrentDirectory(), "Data", DataFolder));

        Path.Combine(Directory.GetCurrentDirectory(), "Data", DataFolder).CopyToTempDirectory();

        await OnGenerate();

        var pdfBytes = await this.GetFields<string>()!.GeneratePdf();

        return File(pdfBytes, "application/pdf", FileName + ".pdf");
    }

    public async Task<IActionResult> OnPostImportAsync()
    {
        if (UploadedFile == null || UploadedFile.Length == 0)
        {
            ModelState.AddModelError("", "Please select a valid file.");
            return Page();
        }

        using var reader = new StreamReader(UploadedFile.OpenReadStream());

        var fileContent = await reader.ReadToEndAsync();

        return SetToPage(fileContent);
    }

    public async Task<IActionResult> OnPostLoadExampleAsync()
    {
        var file = Path.Combine(Directory.GetCurrentDirectory(), "Data", DataFolder, "fake.json");

        using var reader = new StreamReader(file);

        var fileContent = await reader.ReadToEndAsync();

        return SetToPage(fileContent);
    }

    public void OnSet() => Save(GetType().Name, Serialize());

    public void OnGet() => Deserialize(Load(GetType().Name));

    public string Serialize() =>
        JsonSerializer.Serialize(_props.ToDictionary(p => p.Name, p => p.GetValue(this)), options);

    public void Deserialize(string? json)
    {
        if (json is null || JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) is not Dictionary<string, JsonElement> dict)
            return;

        foreach (var prop in _props)
            prop.SetValue(this, dict[prop.Name].Deserialize(prop.PropertyType));
    }

    protected abstract Task OnGenerate();

    private IActionResult SetToPage(string? json)
    {
        if (json == null)
            return Page();

        try { Save(GetType().Name, json); }
        catch (Exception) { ModelState.AddModelError("", "Invalid file format."); }

        return RedirectToPage();
    }

    private IEnumerable<PropertyInfo> GetProperties() => GetType()
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(prop => Attribute.IsDefined(prop, typeof(BindPropertyAttribute)) && !prop.PropertyType.IsInterface);

    private void Save(string name, string value) => HttpContext.Session.SetString(name, value);

    private string? Load(string name) => HttpContext.Session.GetString(name);
}