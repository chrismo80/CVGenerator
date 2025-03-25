namespace CVGenerator.LaTeX;

public static class PdfGenerator
{
    const string PREFIX = "@";
    const string FILE = "cv";
    static readonly string folder = Directory.GetCurrentDirectory();
    private static readonly string tempFolder = Path.Combine(folder, "temp");
    static readonly string template = Path.Combine(folder, "Data", "template.tex");
    static readonly string srcTex = Path.Combine(folder, "Data", FILE + ".tex");
    static readonly string mainTex = Path.Combine(tempFolder, FILE + ".tex");
    static readonly string tempTex = Path.Combine(tempFolder, "template.tex");
    static readonly string tempPdf = Path.Combine(tempFolder, FILE + ".pdf");
    static readonly string pdfUrl = Path.Combine("wwwroot", FILE + ".pdf");

    static readonly System.Diagnostics.ProcessStartInfo startInfo = new()
    {
        FileName = "pdflatex",
        Arguments = $"-output-directory={tempFolder} {mainTex}",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = false
    };

    public static async Task<byte[]> GeneratePdf(this Dictionary<string, object> input)
    {
        Directory.CreateDirectory(tempFolder);

        File.Copy(srcTex, mainTex, true);

        var info = await File.ReadAllTextAsync(template);

        await File.WriteAllTextAsync(tempTex, info.FillTemplate(input));

        await System.Diagnostics.Process.Start(startInfo)!.RenderPdf();

        File.Copy(tempPdf, pdfUrl, true);

        Directory.Delete(tempFolder, true);

        return await File.ReadAllBytesAsync(pdfUrl);
    }

    private static async Task RenderPdf(this System.Diagnostics.Process process)
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