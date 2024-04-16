using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var b = ResourceBuilder.CreateDefault();
b
    .AddService("ServiceA")
    .AddAttributes(new[]
    {
        new KeyValuePair<string, object>("some-attrib","some-value")
    });

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddOpenTelemetry(o =>
    {
        o
            .SetResourceBuilder(b)
            .AddOtlpExporter();
    });
});

builder.Services.AddOpenTelemetry()
    .WithMetrics(meterProviderBuilder =>
        {
            meterProviderBuilder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .SetResourceBuilder(b)
                .AddOtlpExporter();
        }
    )
    .WithTracing(tracerProviderBuilder =>
        {
            tracerProviderBuilder
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
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


app.MapGet("/myendpoint", () =>
    {
        return "hello";
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();
