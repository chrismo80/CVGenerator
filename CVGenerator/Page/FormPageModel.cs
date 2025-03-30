using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using System.Reflection;
using System.Text;
using System.Text.Json;

namespace CVGenerator.Pages;

public class FormPageModel : PageModel
{
	private IEnumerable<PropertyInfo> _props;

	public FormPageModel() => _props = GetProperties();

	public async Task<IActionResult> OnPostExportAsync() =>
		File(Encoding.UTF8.GetBytes(Serialize()), "application/json", GetType().Name + ".json");

	public void OnSet() =>
		Save(GetType().Name, Serialize());

	public void OnGet() =>
		Deserialize();

	public void Deserialize()
	{
		if (Load(GetType().Name) is not string json)
			return;

		var dict = JsonSerializer.Deserialize<Dictionary<string,JsonElement>>(json);

		foreach (var prop in _props)
			prop.SetValue(this, dict[prop.Name].Deserialize(prop.PropertyType));
	}

	public string Serialize()
	{
		var dict = _props.ToDictionary(p => p.Name, p => p.GetValue(this));

		return JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
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