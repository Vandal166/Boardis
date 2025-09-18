using Application;
using Infrastructure;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json.Serialization;
using Serilog;
using Web.API;
using Web.API.Common;
using Web.API.Communication.Hubs;

var builder = WebApplication.CreateBuilder(args);
IdentityModelEventSource.ShowPII = true;

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; // Handle reference loops
        options.SerializerSettings.ContractResolver = new DefaultContractResolver(); // default property naming (PascalCase)
    });

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

app.MapHub<BoardNotificationHub>("/boardHub");
app.MapHub<GeneralNotificationHub>("/generalNotificationHub");


app.Run();


public partial class Program;