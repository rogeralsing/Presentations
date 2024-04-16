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
b
    .AddService("ServiceA")
    .AddAttributes(new[]
    {
        new KeyValuePair<string, object>("some-attrib","some-value")
    });

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


app.MapGet("/myendpoint", () =>
    {
        //start a new span manually
        using var a = activitySource.StartActivity("MySpan");
        //if we have a span, add tags, events and baggage
        a?.AddTag("some-tag", "tag value");
        a?.AddEvent(new ActivityEvent("something happened"));
        a?.AddBaggage("some-baggage", "hello world");
        
        return "hello";
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();