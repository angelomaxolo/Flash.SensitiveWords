using Flash.SensitiveWords.API.Endpoints;
using Flash.SensitiveWords.API.Middleware;
using Flash.SensitiveWords.Application.Extensions;
using Flash.SensitiveWords.Infrastructure.Extensions;
using Flash.SensitiveWords.Infrastructure.Seeding;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder);

var app = builder.Build();
await InitializeDatabaseAsync(app);
ConfigurePipeline(app);
MapEndpoints(app);
app.Run();

static void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplicationServices();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
        options.IncludeXmlComments(xmlFilePath, includeControllerXmlComments: true);
    });
}

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.InitializeAsync();
}

static void ConfigurePipeline(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRequestLogging();
    app.UseHttpsRedirection();
}

static void MapEndpoints(WebApplication app)
{
    app.MapSensitiveWordsEndpoints();
}
