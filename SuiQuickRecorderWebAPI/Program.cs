using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SuiQuickRecorderCore.Models.KeyValue;
using SuiQuickRecorderCore.Models.Origin;
using SuiQuickRecorderCore.Services;
using SuiQuickRecorderWebAPI.Data;
using SuiQuickRecorderWebAPI.Models;
using SuiQuickRecorderWebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.InputFormatters.Add(new SuiQuickRecorderWebAPI.Formatters.TextPlainInputFormatter());
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(s =>
{
    s.AddPolicy("default", p =>
    {
        p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});

// Configure SuiQuickRecorder
builder.Services.Configure<SuiWebAPIOptions>(
    builder.Configuration.GetSection("SuiQuickRecorder"));

builder.Services.AddDbContext<SuiContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register SuiSessionService as Singleton to hold shared HttpClient and session state
builder.Services.AddSingleton<SuiSessionService>();

// Register SuiMetadataService
builder.Services.AddScoped<SuiMetadataService>();
builder.Services.AddScoped<SuiQuickRecorderService>();

// MetadataService (WebAPI) no longer needs typed HttpClient injected, it depends on SuiMetadataService
builder.Services.AddScoped<MetadataService>();
builder.Services.AddScoped<RecordsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

app.UseCors("default");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
