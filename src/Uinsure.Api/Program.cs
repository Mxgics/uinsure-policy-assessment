using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uinsure.Api.Errors;
using Uinsure.Api.Persistence;
using Uinsure.Api.Policies;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(allowIntegerValues: false)));

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problem = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Type = "https://httpstatuses.com/400"
        };
        ApiProblemDetailsDefaults.AddExtensions(problem, context.HttpContext, "validation_error");

        return new BadRequestObjectResult(problem)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
        ApiProblemDetailsDefaults.AddExtensions(
            context.ProblemDetails,
            context.HttpContext,
            ApiProblemDetailsDefaults.CodeFor(context.ProblemDetails.Status));
});
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddOpenApi("v1");
builder.Services.AddHealthChecks();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IPolicyReferenceGenerator, PolicyReferenceGenerator>();
builder.Services.AddScoped<PolicyService>();
builder.Services.AddDbContext<UinsureDbContext>((services, options) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("Uinsure");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "ConnectionStrings:Uinsure is required when database services are used.");
    }

    options.UseSqlServer(connectionString);
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapOpenApi();
app.MapControllers();

app.Run();

public partial class Program;
