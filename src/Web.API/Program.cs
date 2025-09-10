using Application;
using Infrastructure;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json.Serialization;
using Serilog;
using Web.API;

var builder = WebApplication.CreateBuilder(args);
IdentityModelEventSource.ShowPII = true;

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; // Handle reference loops
        options.SerializerSettings.ContractResolver = new DefaultContractResolver(); // default property naming (PascalCase)
    })
    .AddJsonOptions(_ => {});

builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // cookies/session
});

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddWebAPI(builder.Configuration);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
});

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program { }