using System.Diagnostics;

namespace CVGenerator.LaTeX;

public static class PdfGenerator
{
    const string PREFIX = "@";

    public static string TempFolder => Path.Combine(Directory.GetCurrentDirectory(), "temp");

    public static async Task<byte[]> GeneratePdf(this Dictionary<string, object> input,
        string main = "document")
    {
        var mainFile = Path.Combine(TempFolder, main);

        var info = await File.ReadAllTextAsync(mainFile + ".tex");

        await File.WriteAllTextAsync(mainFile + ".tex", info.FillTemplate(input));

        var startInfo = new ProcessStartInfo()
        {
            FileName = "pdflatex",
            Arguments = $"-output-directory={TempFolder} {mainFile}.tex",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        await Process.Start(startInfo)!.RenderPdf();

        var data = await File.ReadAllBytesAsync($"{mainFile}.pdf");

        Directory.Delete(TempFolder, true);

        return data;
    }

    public static async Task SaveAsyncToTempFolder(this IFormFile? image, string filename)
    {
        if (image == null)
            return;

        await using var fileStream = new FileStream(Path.Combine(TempFolder, filename), FileMode.Create);

        await image.CopyToAsync(fileStream);
    }

    private static async Task RenderPdf(this Process process)
    {
        process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
        process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
    }

    private static string FillTemplate(this string content, Dictionary<string, object> input)
    {
        foreach (var (name, value) in input)
            content = content.Replace(PREFIX + name, value?.ToString()?.Sanitize());

        return content;
    }
}