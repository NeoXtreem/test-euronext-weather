using Euronext.Weather.Data;
using Euronext.Weather.Models;
using Euronext.Weather.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer").AddJwtBearer();

builder.Services.AddDbContext<WeatherForecastContext>(options => options.UseInMemoryDatabase("weatherForecasts"));
builder.Services.AddScoped<WeatherForecastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/weatherforecast", (WeatherForecastService weatherForecastService, WeatherForecast weatherForecast) => weatherForecastService.AddWeatherForecast(weatherForecast))
    .RequireAuthorization(p => p.RequireRole("weatherman").RequireClaim("scope", "weather"))
    .WithName("AddWeatherForecast")
    .WithOpenApi();

app.MapGet("/weekweatherforecast", (WeatherForecastService weatherForecastService, [FromQuery] DateOnly startDate) => weatherForecastService.GetWeekWeatherForecast(startDate))
    .RequireAuthorization(p => p.RequireRole("reader").RequireClaim("scope", "weather"))
    .AddEndpointFilter(async (c, n) => c.GetArgument<DateOnly>(1) < DateOnly.FromDateTime(DateTime.Today) ? Results.Problem("Past dates not allowed!") : await n(c))
    .WithName("GetWeekWeatherForecast")
    .WithOpenApi();

app.Run();

public partial class Program { }