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

    private readonly IEnumerable<PropertyInfo> _props;

    public abstract string DataFolder { get; }

    public abstract string FileName { get; }

    [BindProperty] public IFormFile? UploadedFile { get; set; }

    protected DocumentPageModel() => _props = GetProperties();

    public async Task<IActionResult> OnPostAsync() =>
        Request.Form["handler"].ToString() switch
        {
            "Import" => await OnPostImportAsync(),
            "Export" => OnPostExport(),
            "Generate" => await OnPostGenerateAsync(),
            _ => Page()
        };

    public async Task<IActionResult> OnPostGenerateAsync()
    {
        OnSet();

        Path.Combine(Directory.GetCurrentDirectory(), "Data", DataFolder).CopyToTempDirectory();

        await OnGenerate();

        var pdfBytes = await this.GetFields<string>()!.GeneratePdf();

        return File(pdfBytes, "application/pdf", FileName + ".pdf");
    }

    public IActionResult OnPostExport() =>
        File(Encoding.UTF8.GetBytes(Serialize()), "application/json", GetType().Name + ".json");

    public async Task<IActionResult> OnPostImportAsync()
    {
        if (UploadedFile == null || UploadedFile.Length == 0)
        {
            ModelState.AddModelError("", "Please select a valid file.");
            return Page();
        }

        using var reader = new StreamReader(UploadedFile.OpenReadStream());

        var fileContent = await reader.ReadToEndAsync();

        try { Save(GetType().Name, fileContent); }
        catch (Exception) { ModelState.AddModelError("", "Invalid file format."); }

        return RedirectToPage();
    }

    public void OnSet() =>
        Save(GetType().Name, Serialize());

    public void OnGet() =>
        Deserialize(Load(GetType().Name));

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

    private IEnumerable<PropertyInfo> GetProperties() => GetType()
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(prop => !prop.PropertyType.IsInterface)
        .Where(prop => Attribute.IsDefined(prop, typeof(BindPropertyAttribute)));

    private void Save(string name, string value) =>
        HttpContext.Session.SetString(name, value);

    private string? Load(string name) =>
        HttpContext.Session.GetString(name);
}