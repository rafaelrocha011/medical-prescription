using Microsoft.AspNetCore.OData;
using MedicalPrescription.Api.Errors;
using MedicalPrescription.Infrastructure;
using MedicalPrescription.Api.Configuration;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddApplicationUseCases();
builder.Services.AddInfrastructure(builder.Configuration);

// Add services to the container.

builder.Services.AddControllers()
    .AddOData(options =>
    {
        options.TimeZone = TimeZoneInfo.Utc;

        options
        .Select()
        .Filter()
        .OrderBy()
        .Count()
        .SetMaxTop(100)
        .AddRouteComponents(
            "odata",
            ODataEdmModel.Criar());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    await DevelopmentDataSeeder.SeedAsync(app.Services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();



