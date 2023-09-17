using Euronext.Weather.Data;
using Euronext.Weather.Models;
using Euronext.Weather.Services;
using Euronext.Weather.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<ForecastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/forecast", (ForecastService forecastService, MessageService _, Forecast forecast) => forecastService.AddForecast(forecast))
    .RequireAuthorization(p => p.RequireRole("weatherman").RequireClaim("scope", "weather"))
    .AddEndpointFilter(async (efiContext, next) => DateOnlyValidation.IsTodayOrLater(efiContext.GetArgument<Forecast>(2).Date) ? await next(efiContext) : Results.Problem(efiContext.GetArgument<MessageService>(1).GetPastDatesErrorMessage()))
    .AddEndpointFilter(async (efiContext, next) => DateOnlyValidation.IsTodayOrLater(efiContext.GetArgument<Forecast>(2).Date) ? await next(efiContext) : Results.Problem(efiContext.GetArgument<MessageService>(1).GetPastDatesErrorMessage()))
    .WithName("AddForecast")
    .WithOpenApi();

app.MapGet("/weekforecast/{startDate}", (ForecastService forecastService, MessageService _, [FromRoute] DateOnly startDate) => forecastService.GetWeekForecast(startDate))
    .RequireAuthorization(p => p.RequireRole("reader").RequireClaim("scope", "weather"))
    .AddEndpointFilter(async (efiContext, next) => DateOnlyValidation.IsTodayOrLater(efiContext.GetArgument<DateOnly>(2)) ? await next(efiContext) : Results.Problem(efiContext.GetArgument<MessageService>(1).GetPastDatesErrorMessage()))
    .WithName("GetWeekForecast")
    .WithOpenApi();
app.Run();

public partial class Program { }