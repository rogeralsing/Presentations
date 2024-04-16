using System.Diagnostics;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



//this defines your resource, meaning the service name, and various associated attribs
var b = ResourceBuilder.CreateDefault();
b.AddService("ServiceB");

//this is used to create manual spans
var activitySource = new ActivitySource("some-name");

//configure logging to push to Otel
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddOpenTelemetry(o =>
    {
        o
            .SetResourceBuilder(b)
            .AddOtlpExporter();
    });
});

//add Otel services
builder.Services.AddOpenTelemetry()
    //configure metrics
    .WithMetrics(meterProviderBuilder =>
        {
            meterProviderBuilder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .SetResourceBuilder(b)
                .AddOtlpExporter();
        }
    )
    //configure tracing
    .WithTracing(tracerProviderBuilder =>
        {
            tracerProviderBuilder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource(activitySource.Name)
                .SetResourceBuilder(b)
                .AddOtlpExporter();
        }
    );


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}