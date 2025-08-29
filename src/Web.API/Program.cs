using Application;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using Web.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("SwaggerOnly", builder =>
    {
        builder.WithOrigins("http://localhost:5185") // Swagger UI origin
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddWebAPI(builder.Configuration);

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
app.UseCors("SwaggerOnly");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();