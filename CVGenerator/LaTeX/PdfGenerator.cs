using System.Diagnostics;

namespace CVGenerator.LaTeX;

public static class PdfGenerator
{
    const string PREFIX = "@";

    static readonly string cd = Directory.GetCurrentDirectory();

    private static readonly string tempFolder = Path.Combine(cd, "temp");

    public static async Task<byte[]> GeneratePdf(this Dictionary<string, object> input,
        string dataFolder, string main = "main", string template = "template")
    {
        var templateFile = Path.Combine(tempFolder, template + ".tex");

        var info = await File.ReadAllTextAsync(templateFile);

        await File.WriteAllTextAsync(templateFile, info.FillTemplate(input));

        var mainFile = Path.Combine(tempFolder, main);

        var startInfo = new ProcessStartInfo()
        {
            FileName = "pdflatex",
            Arguments = $"-output-directory={tempFolder} {mainFile}.tex",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = false
        };

        await Process.Start(startInfo)!.RenderPdf();

        var data = await File.ReadAllBytesAsync($"{mainFile}.pdf");

        //Directory.Delete(tempFolder, true);

        return data;
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