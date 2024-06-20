using AnalyticsWebApi.Datamodel;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;
using AnalyticsWebApi;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("AnalyticsDb")))
{
    Activity.Current?.AddEvent(new ActivityEvent("ERROR: Unable to detect database connection"));
    throw new InvalidOperationException("The database connection string 'AnalyticsDb' is not configured.");
}

builder.Services.AddSingleton<AnalyticsRepository>(_ => new AnalyticsRepository(builder.Configuration.GetConnectionString("AnalyticsDb")!));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

var app = builder.Build();
app.UseCors("AllowAllOrigins");
app.MapDefaultEndpoints();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/Analytics/HelloWorld", () => "Hello, world!")
    .WithOpenApi();
app.MapGet("/Analytics/Hello", (HttpContext ctx) => $"Hello, {ctx.Connection.RemoteIpAddress}!")
    .WithOpenApi();
app.MapGet("/Analytics/ReferralUrl", (HttpContext ctx) => ctx.Request.Headers.Referer.ToString())
    .WithOpenApi();
app.MapPost("/Analytics/SaveAnalytics", async (string key, string data, HttpContext httpContext, ILogger<Program> logger, AnalyticsRepository repository) =>
{
    DateTime current = DateTime.UtcNow;
    Activity.Current?.AddEvent(new ActivityEvent($"Received a request to SaveAnalytics"));

    // Retrieve the client's IP address
    string? remoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString();

    httpContext.Request.Headers.TryGetValue("Referer", out Microsoft.Extensions.Primitives.StringValues referer);
    string refererString = referer.ToString() ?? string.Empty;
    if (string.IsNullOrWhiteSpace(refererString))
    {
        return Results.BadRequest("Can't save data without a referer.");
    }
    int response = await repository.SaveAnalyticsData(new AnalyticsData
    {
        DataKey = key,
        Data = data,
        Referer = referer,
        CreatedAt = current,
        IpAddress = remoteIpAddress // Save the IP address
    });
    if (response != 1)
    {
        return Results.StatusCode(500);
    }
    return Results.Ok($"Message received at {current} from IP: {remoteIpAddress}.");

})
    .WithOpenApi();
app.MapGet("/Analytics/GetAnalyticsData", async (string? key, HttpContext httpContext, ILogger<Program> logger, AnalyticsRepository repository) =>
{
    Activity.Current?.AddEvent(new ActivityEvent($"Received a request to GetAnalyticsData with key: {key}"));
    int delay = Random.Shared.Next(0, 500);
    await Task.Delay(delay);
    logger.LogDebug("Waited for {delay} ms before returning the response", delay);
    httpContext.Request.Headers.TryGetValue("Referer", out Microsoft.Extensions.Primitives.StringValues referer);
    string refererString = string.IsNullOrWhiteSpace(referer.ToString()) ? string.Empty : referer.ToString();
    string? remoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
    if (string.IsNullOrWhiteSpace(refererString))
    {
        return Results.BadRequest("Can't get data without a referer.");
    }
    AnalyticsData? dataWithKey = await repository.GetAnalyticsDataByKey(key ?? "", refererString, remoteIpAddress);
    if (dataWithKey != null)
    {
        return Results.Ok(dataWithKey);
    }
    else
    {
        return Results.NotFound($"No data found with key: {key}");
    }
})
    .WithOpenApi();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
