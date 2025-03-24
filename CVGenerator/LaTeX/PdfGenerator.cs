using System.Collections;
using System.Diagnostics;

namespace CVGenerator.LaTeX;

public static class PdfGenerator
{
    const string PREFIX = "@";
    const string FILE = "cv";
    static readonly string folder = Directory.GetCurrentDirectory();
    private static readonly string tempFolder = Path.Combine(folder, "temp");
    static readonly string template = Path.Combine(folder, "Data", "template.tex");
    static readonly string tempTex = Path.Combine(tempFolder, FILE + ".tex");
    static readonly string tempPdf = Path.Combine(tempFolder, FILE + ".pdf");
    static readonly string pdfUrl = Path.Combine("wwwroot", FILE + ".pdf");

    static readonly ProcessStartInfo startInfo = new()
    {
        FileName = "pdflatex",
        Arguments = $"-output-directory={tempFolder} {tempTex}",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    public static async Task<byte[]> GeneratePdf(this Dictionary<string, object> input)
    {
        Directory.CreateDirectory(tempFolder);

        var cv = await File.ReadAllTextAsync(template);

        await File.WriteAllTextAsync(tempTex, cv.FillTemplate(input));

        await Process.Start(startInfo)!.WaitForExitAsync();

        File.Copy(tempPdf, pdfUrl, true);

        Directory.Delete(tempFolder, true);

        return await File.ReadAllBytesAsync(pdfUrl);
    }

    private static string FillTemplate(this string content, Dictionary<string, object> input)
    {
        foreach (var (name, value) in input)
            content = content.Replace(PREFIX + name, value.ToString());

        return content;
    }

    private static string Sanitize(this string input)
    {
        foreach (var c in "#$%&_{}")
            input = input.Replace(c.ToString(), @$"\{c}");

        return input
            .Replace(@"\", @"\textbackslash{}")
            .Replace("~", @"\textasciitilde{}")
            .Replace("^", @"\textasciicircum{}");
    }
}