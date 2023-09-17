using Euronext.Weather.Data;
using Euronext.Weather.Models;
using Euronext.Weather.Services;
using Euronext.Weather.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Bearer").AddJwtBearer();
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

builder.Services.AddDbContext<ForecastContext>(options => options.UseInMemoryDatabase("forecasts"));
builder.Services.AddScoped<ForecastService>();

var app = builder.Build();

using (var serviceScope = app.Services.CreateScope())
{
    LocalizerService.Localizer = serviceScope.ServiceProvider.GetRequiredService<IStringLocalizerFactory>().Create(typeof(LocalizerService));
}

var options = builder.Configuration.GetSection("Options");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/forecast", (ForecastService forecastService, Forecast forecast) => forecastService.AddForecast(forecast))
    .RequireAuthorization(p => p.RequireRole("weatherman").RequireClaim("scope", "weather"))
    .AddEndpointFilter(async (efiContext, next) => DateOnlyValidation.IsTodayOrLater(efiContext.GetArgument<Forecast>(1).Date) ? await next(efiContext) : Results.Problem(LocalizerService.GetPastDatesErrorMessage()))
    .AddEndpointFilter(async (efiContext, next) =>
    {
        var temperatureC = efiContext.GetArgument<Forecast>(1).TemperatureC;
        return temperatureC < options.GetValue<int>("MinTemperature") || temperatureC > options.GetValue<int>("MaxTemperature")
            ? Results.Problem(LocalizerService.GetTemperatureOutOfRangeErrorMessage())
            : await next(efiContext);
    })
    .WithName("AddForecast")
    .WithOpenApi();

app.MapGet("/weekforecast/{startDate}", (ForecastService forecastService, [FromRoute] DateOnly startDate) => forecastService.GetWeekForecast(startDate))
    .RequireAuthorization(p => p.RequireRole("reader").RequireClaim("scope", "weather"))
    .AddEndpointFilter(async (efiContext, next) => DateOnlyValidation.IsTodayOrLater(efiContext.GetArgument<DateOnly>(1)) ? await next(efiContext) : Results.Problem(LocalizerService.GetPastDatesErrorMessage()))
    .WithName("GetWeekForecast")
    .WithOpenApi();

app.Run();

public partial class Program { }