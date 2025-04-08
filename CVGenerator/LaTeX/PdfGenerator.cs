using System.Diagnostics;

namespace CVGenerator.LaTeX;

public static class PdfGenerator
{
    const string PREFIX = "@";

    public static string TempFolder => Path.Combine(Directory.GetCurrentDirectory(), "temp");

    public static async Task<byte[]> GeneratePdf(this Dictionary<string, object> input,
        string subFolder, string main = "document")
    {
        var tempFolder = Path.Combine(TempFolder, subFolder);

        var mainFile = Path.Combine(tempFolder, main);

        var document = await File.ReadAllTextAsync(mainFile + ".tex");

        await File.WriteAllTextAsync(mainFile + ".tex", document.FillTemplate(input));

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

        Directory.Delete(tempFolder, true);

        return data;
    }

    public static void CopyToTempDirectory(this string dataDirectory, string subFolder)
    {
        var tempFolder = Path.Combine(TempFolder, subFolder);

        Directory.CreateDirectory(tempFolder);

        foreach (string dir in Directory.GetDirectories(dataDirectory, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dir.Replace(dataDirectory, tempFolder));

        foreach (string file in Directory.GetFiles(dataDirectory, "*.*", SearchOption.AllDirectories))
            File.Copy(file, file.Replace(dataDirectory, tempFolder), true);
    }

    public static async Task SaveAsyncToTempFolder(this IFormFile? image, string subFolder, string filename)
    {
        if (image == null)
            return;

        await using var fileStream = new FileStream(Path.Combine(TempFolder, subFolder, filename), FileMode.Create);

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