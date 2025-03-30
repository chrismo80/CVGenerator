using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using System.Reflection;
using System.Text;
using System.Text.Json;

using CVGenerator.LaTeX;

namespace CVGenerator.Pages;

public abstract class DocumentPageModel : PageModel
{
	public abstract string DataFolder { get; }
	public abstract string FileName { get; }

	protected abstract Task OnGenerate();

	[BindProperty] public IFormFile? UploadedFile { get; set; }

	private IEnumerable<PropertyInfo> _props;

	protected DocumentPageModel() => _props = GetProperties();

	public async Task<IActionResult> OnPostAsync() =>
		Request.Form["handler"].ToString() switch
		{
			"Import" => await OnPostImportAsync(),
			"Export" => await OnPostExportAsync(),
			"Generate" => await OnPostGenerateAsync(),
			_ => Page()
		};

	public async Task<IActionResult> OnPostGenerateAsync()
	{
		OnSet();

		Directory.CreateDirectory("temp");

		await OnGenerate();

		var pdfBytes = await this.GetFields<string>()!.GeneratePdf(DataFolder);

		return File(pdfBytes, "application/pdf", FileName + ".pdf");
	}

	public async Task<IActionResult> OnPostExportAsync() =>
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

		try
		{
			Save(GetType().Name, fileContent);
		}
		catch (Exception ex)
		{
			ModelState.AddModelError("", "Invalid file format.");
		}

		return RedirectToPage();
	}

	public void OnSet() =>
		Save(GetType().Name, Serialize());

	public void OnGet()
	{
		ModelState.Clear();
		Deserialize(Load(GetType().Name));
	}


	public string Serialize()
	{
		var dict = _props.ToDictionary(p => p.Name, p => p.GetValue(this));

		return JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
	}

	public void Deserialize(string? json)
	{
		if (json is null)
			return;

		var dict = JsonSerializer.Deserialize<Dictionary<string,JsonElement>>(json);

		foreach (var prop in _props)
			prop.SetValue(this, dict[prop.Name].Deserialize(prop.PropertyType));
	}

	private IEnumerable<PropertyInfo> GetProperties() => GetType()
		.GetProperties(BindingFlags.Public | BindingFlags.Instance)
		.Where(prop => !prop.PropertyType.IsInterface)
		.Where(prop => Attribute.IsDefined(prop, typeof(BindPropertyAttribute)));

	private void Save(string name, string value) =>
		HttpContext.Session.SetString(name, value);

	private string? Load(string name) =>
		HttpContext.Session.GetString(name);
}