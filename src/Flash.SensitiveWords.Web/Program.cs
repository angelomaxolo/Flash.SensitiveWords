using Flash.SensitiveWords.RestClient.Extensions;

var builder = WebApplication.CreateBuilder(args);

// AppInsights and application services
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllersWithViews();

// Authentication (cookie-based) for admin UI
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/account/login";
        options.LogoutPath = "/account/logout";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

// RestClient registration (will include API key header if configured)
builder.Services.AddRestClient(builder.Configuration);

var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Required for CSS/JS/images
app.UseStaticFiles();

app.UseRouting();

// Future-proof (admin section)
app.UseAuthentication();
app.UseAuthorization();

// MVC routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Chat}/{action=Index}/{id?}");

app.Run();