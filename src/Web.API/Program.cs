using System.Globalization;
using Application;
using Infrastructure;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json.Serialization;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using Serilog.Sinks.Loki;
using Serilog.Sinks.OpenTelemetry;
using Web.API;
using Web.API.Common;
using Web.API.Communication.Hubs;

var builder = WebApplication.CreateBuilder(args);
IdentityModelEventSource.ShowPII = false;

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; // Handle reference loops
        options.SerializerSettings.ContractResolver = new DefaultContractResolver(); // default property naming (PascalCase)
    });

builder.Services.AddLocalization();

builder.Services.AddSignalR();

builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
policy => policy.WithOrigins("http://localhost")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // cookies/session
});

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddWebAPI(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Enum.GetNames<Domain.Constants.Permissions>())
    {
        options.AddPolicy(permission, policy => policy.Requirements.Add(new PermissionRequirement(permission)));
    }
});

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
        .Enrich.WithProperty("service_name", "web-api")
        .WriteTo.LokiHttp(() => new LokiSinkConfiguration
        {
            LokiUrl = "http://loki:3100"
        });
});

builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeScopes = true;
    options.ParseStateValues = true;
    options.IncludeFormattedMessage = true;
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("web-api", serviceVersion: "1.0.0"))
    .WithLogging(options =>
    {
        options.AddOtlpExporter(otlp => 
        {
            otlp.Endpoint = new Uri("http://jaeger:4318/v1/logs");
            otlp.Protocol = OtlpExportProtocol.HttpProtobuf;
        });
    })
    .WithMetrics(options =>
    {
        options.AddAspNetCoreInstrumentation();
        options.AddHttpClientInstrumentation();
        options.AddPrometheusExporter();
        options.AddOtlpExporter(otlp => 
        {
            otlp.Endpoint = new Uri("http://jaeger:4318/v1/metrics");
            otlp.Protocol = OtlpExportProtocol.HttpProtobuf;
        });
    })
    .WithTracing(options =>
    {
        options.AddAspNetCoreInstrumentation();
        options.AddHttpClientInstrumentation();
        options.AddOtlpExporter(otlp => 
        {
            otlp.Endpoint = new Uri("http://jaeger:4318/v1/traces");  // Explicit path for traces
            otlp.Protocol = OtlpExportProtocol.HttpProtobuf;  // Force HTTP
        });
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors("AllowFrontend");
app.UseOpenTelemetryPrometheusScrapingEndpoint();

var supportedCultures = new[]
{
    new CultureInfo("en"), // English (default)
    new CultureInfo("pl")  // Polish
};
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<BoardNotificationHub>("/boardHub");
app.MapHub<GeneralNotificationHub>("/generalNotificationHub");


app.Run();


public partial class Program;