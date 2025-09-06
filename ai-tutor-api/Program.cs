using System.Globalization;
using Ai.Tutor.Api.DTOs;
using Ai.Tutor.Api.Seeding;
using Ai.Tutor.Api.Services;
using Ai.Tutor.Api.Validators;
using Ai.Tutor.Contracts.DTOs;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Infrastructure.Data;
using Ai.Tutor.Services.Features.Folders;
using Ai.Tutor.Services.Mediation;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logging: Serilog
builder.Host.UseSerilog(
    (ctx, services, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration)
      .ReadFrom.Services(services));

// Controllers (no minimal APIs)
builder.Services.AddControllers();

// SignalR
builder.Services.AddSignalR();

// Localization (en default, extensible)
builder.Services.AddLocalization();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddScoped<IValidator<ListMessagesQueryParams>, ListMessagesRequestValidator>();

builder.Services.AddScoped<IValidator<CreateMessageRequest>, CreateMessageRequestValidator>();

// ProblemDetails (Hellang) via extension
builder.Services.AddApiProblemDetails(builder.Environment);

// DbContext (Postgres)
var connString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AiTutorDbContext>(opt => opt.UseNpgsql(connString));

// Repositories (DI)
builder.Services.AddRepositories();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

// Application services
builder.Services.AddApplicationServices();

// Custom Mediator
builder.Services.AddScoped<IMediator, Mediator>();

// Register handlers by assembly scanning (ai-tutor-services)
builder.Services.AddMediatorHandlersFromAssembly(typeof(DeleteFolderHandler).Assembly);

// Seeding: disable in Testing environment so integration tests control their own data
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHostedService<StartupSeeder>();
}

// OpenAPI for DTOs/problem schema
builder.Services.AddOpenApi();

var app = builder.Build();

// Middleware ordering: CorrelationId -> ProblemDetails
app.UseCorrelationId();

app.UseApiProblemDetails();

// Localization
var supportedCultures = new[] { new CultureInfo("en") };

app.UseRequestLocalization(
    new RequestLocalizationOptions
    {
        DefaultRequestCulture = new RequestCulture("en"),
        SupportedCultures = supportedCultures,
        SupportedUICultures = supportedCultures,
    });

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

// Map controllers and SignalR hub placeholder
app.MapControllers();

app.MapHub<Microsoft.AspNetCore.SignalR.Hub>("/hubs/threads");

app.Run();

namespace Ai.Tutor.Api
{
    public partial class Program
    {
    }
}