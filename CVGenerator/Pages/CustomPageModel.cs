using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using CVGenerator.LaTeX;

namespace CVGenerator.Pages;

public class CustomPageModel : PageModel
{
	private string _typeName;

	JsonSerializerOptions _options = new()
	{
		WriteIndented = true,

		TypeInfoResolver = new DefaultJsonTypeInfoResolver
		{
			Modifiers = { MyContract }
		}
	};

	public CustomPageModel()
	{
		_typeName = GetType().Name;
	}

	public async Task<IActionResult> OnPostExportAsync()
	{
		var text = JsonSerializer.Serialize(this, _options);

		var bytes = Encoding.UTF8.GetBytes(text);

		return File(bytes, "application/json", _typeName + ".json");
	}

	public void OnSet()
	{
		foreach (var (name, value) in this.GetBindedFields<string, BindPropertyAttribute>())
			Save(_typeName + name, value ?? "");

		foreach (var (name, value) in this.GetBindedFields<List<Info>, BindPropertyAttribute>().Where(kvp => kvp.Value!.Count > 0))
			Save(_typeName + name, JsonSerializer.Serialize(value));

		foreach (var (name, value) in this.GetBindedFields<List<Skill>, BindPropertyAttribute>().Where(kvp => kvp.Value!.Count > 0))
			Save(_typeName + name, JsonSerializer.Serialize(value));

		return;

		var json = JsonSerializer.Serialize(this, _options);

		Save(GetType().Name, JsonSerializer.Serialize(this, _options));

	}

	public void OnGet()
	{
		foreach (var prop in this.GetBindedProperties<string, BindPropertyAttribute>())
			prop.SetProperty(this, Load(_typeName + prop.Name));

		foreach (var prop in this.GetBindedProperties<List<Info>, BindPropertyAttribute>())
			prop.SetProperty(this, Load(_typeName + prop.Name)?.LoadFromJson<List<Info>>());

		foreach (var prop in this.GetBindedProperties<List<Skill>, BindPropertyAttribute>())
			prop.SetProperty(this, Load(_typeName + prop.Name)?.LoadFromJson<List<Skill>>());

		return;

		var json = Load(GetType().Name);

		if (json == null)
			return;

		var me = JsonSerializer.Deserialize(Load(GetType().Name), GetType(), _options);

		CopyProperties(me, this);

	}

	private static void MyContract(JsonTypeInfo typeInfo)
	{
		var t = typeInfo.Type;

		for (int i = 0; i < typeInfo.Properties.Count; i++)
		{
			if (typeInfo.Properties[i].AttributeProvider is not MemberInfo memberInfo)
				continue;

			if (memberInfo.GetCustomAttribute<BindPropertyAttribute>() != null)
				typeInfo.Properties.RemoveAt(i);
		}
	}

	private static void CopyProperties(object source, object destination)
	{
		var sourceType = source.GetType();
		var destinationType = destination.GetType();

		foreach (var sourceProperty in sourceType.GetProperties())
		{
			var destinationProperty = destinationType.GetProperty(sourceProperty.Name);

			if (destinationProperty == null || !destinationProperty.CanWrite)
				continue;

			destinationProperty.SetValue(destination, sourceProperty.GetValue(source));
		}
	}

	private void Save(string name, string value) =>
		HttpContext.Session.SetString(name, value);

	private string? Load(string name) =>
		HttpContext.Session.GetString(name);
}