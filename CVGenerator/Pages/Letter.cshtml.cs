using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization.Metadata;

using CVGenerator.LaTeX;
using CVGenerator.Extensions;

namespace CVGenerator.Pages;

public class LetterModel(ILogger<LetterModel> logger) : FormPageModel
{
    private readonly ILogger<LetterModel> _logger = logger;

    [BindProperty] public IFormFile? Signature { get; set; }

    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string Street { get; set; } = "Sömmerringplatz 1A";
    [BindProperty] public string City { get; set; } = "55118 Mainz";

    [BindProperty] public string ToName { get; set; } = "Kundenservicecenter";
    [BindProperty] public string ToStreet { get; set; } = "ERGO-Platz 1";
    [BindProperty] public string ToCity { get; set; } = "40477 Düsseldorf";

    [BindProperty] public string Summary { get; set; } = "Riester-Rente-Nr.: LV431512731";
    [BindProperty] public string Dear { get; set; } = "Sehr geehrte Damen und Herren";

    [BindProperty] public string Text { get; set; } =
        "den o. g. Vertrag möchte ich zum nächstmöglichen Zeitpunkt beitragsfrei stellen." +
        "\r\n\r\nBitte schicken Sie mir für die Versicherung mit o. g. Versicherungsscheinnummer einen Nachtrag zu, aus dem die Höhe der beitragsfreien Versicherungssumme und der aktuelle Rückkaufswert hervor geht." +
        "\r\n\r\nDie Nachteile, die mir durch eine Beitragsfreistellung entstehen, sind mir hinreichend bekannt." +
        "\r\n\r\nEine von mir erteilte Einzugsermächtigung ist zum o. g. Datum erloschen." +
        "\r\n\r\nBitte bestätigen Sie mir die Beitragsfreistellung innerhalb der nächsten 14 Tage.";

    public async Task<IActionResult> OnPostGenerateAsync()
    {
        OnSet();

        Directory.CreateDirectory("temp");

        await Signature.SaveAsyncTo(Path.Combine("temp", "signature.png"));

        var pdfBytes = await this.GetFields<string>()!.GeneratePdf("Letter");

        return File(pdfBytes, "application/pdf", ToName + ".pdf");
    }
}