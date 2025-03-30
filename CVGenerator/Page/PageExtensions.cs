namespace CVGenerator.Extensions;

public static class PageExtensions
{
	public static async Task SaveAsyncTo(this IFormFile? image, string path)
	{
		if (image == null)
			return;

		await using var fileStream = new FileStream(path, FileMode.Create);

		await image.CopyToAsync(fileStream);
	}
}