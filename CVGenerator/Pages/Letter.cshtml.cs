using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CVGenerator.Pages;

public class LetterModel : PageModel
{
	private readonly ILogger<LetterModel> _logger;

	public LetterModel(ILogger<LetterModel> logger)
	{
		_logger = logger;
	}

	public void OnGet()
	{ }
}