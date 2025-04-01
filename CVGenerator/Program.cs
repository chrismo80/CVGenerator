var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Set the session timeout
    options.Cookie.HttpOnly = true; // Make session cookie accessible only via HTTP
    options.Cookie.IsEssential = true; // Ensure session works even if cookies are disabled
});

// Register IHttpContextAccessor to access HttpContext
builder.Services.AddHttpContextAccessor();

builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(12345));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Enable session middleware
app.UseSession();  // Add this line to enable session support

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();