using Microsoft.AspNetCore.Mvc;

using CVGenerator.LaTeX;

namespace CVGenerator.Pages;

public class LetterModel(ILogger<CVModel> logger, IHttpContextAccessor httpContextAccessor)
    : DocumentPageModel(logger, httpContextAccessor)
{
    public override string DataFolder => "Letter";

    public override string FileName => ToName;

    [BindProperty] public IFormFile? Signature { get; set; }

    [BindProperty] public string Name { get; set; } = "";

    [BindProperty] public string Street { get; set; } = "";

    [BindProperty] public string City { get; set; } = "";

    [BindProperty] public string ToName { get; set; } = "";

    [BindProperty] public string ToStreet { get; set; } = "";

    [BindProperty] public string ToCity { get; set; } = "";

    [BindProperty] public string Summary { get; set; } = "";

    [BindProperty] public string Dear { get; set; } = "";

    [BindProperty] public string Text { get; set; } = "";

    protected override async Task OnGenerate()
    {
        await Signature.SaveAsyncToTempFolder("signature.png");
    }
}