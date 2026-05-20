using Flash.SensitiveWords.RestClient.Extensions;

var builder = WebApplication.CreateBuilder(args);

// AppInsights and application services
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllersWithViews();

// RestClient registration
builder.Services.AddRestClient(
    builder.Configuration["ApiSettings:BaseUrl"]!);

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