using System.Globalization;
using Ai.Tutor.Api.Seeding;
using Ai.Tutor.Api.Services;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Infrastructure.Data;
using Ai.Tutor.Infrastructure.Repositories;
using Ai.Tutor.Services.Features.Folders;
using Ai.Tutor.Services.Mediation;
using Ai.Tutor.Services.Services;
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

// ProblemDetails (Hellang) via extension
builder.Services.AddApiProblemDetails(builder.Environment);

// DbContext (Postgres)
var connString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AiTutorDbContext>(opt => opt.UseNpgsql(connString));

// Repositories (DI)
builder.Services.AddScoped<IOrgRepository, OrgRepository>();

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IOrgMemberRepository, OrgMemberRepository>();

builder.Services.AddScoped<IFolderRepository, FolderRepository>();

builder.Services.AddScoped<IThreadRepository, ThreadRepository>();

builder.Services.AddScoped<IMessageRepository, MessageRepository>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

// Application services
builder.Services.AddScoped<IOrgDeletionService, OrgDeletionService>();

builder.Services.AddScoped<IUserDeletionService, UserDeletionService>();

// Custom Mediator
builder.Services.AddScoped<IMediator, Mediator>();

// Register handlers by assembly scanning (ai-tutor-services)
builder.Services.AddMediatorHandlersFromAssembly(typeof(DeleteFolderHandler).Assembly);

// Seeding
builder.Services.AddHostedService<StartupSeeder>();

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

app.UseHttpsRedirection();

// Map controllers and SignalR hub placeholder
app.MapControllers();

app.MapHub<Microsoft.AspNetCore.SignalR.Hub>("/hubs/threads");

app.Run();