using System.Globalization;
using Ai.Tutor.Api.Seeding;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Infrastructure.Data;
using Ai.Tutor.Infrastructure.Repositories;
using Ai.Tutor.Services.Services;
using FluentValidation.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers (no minimal APIs)
builder.Services.AddControllers();

// SignalR
builder.Services.AddSignalR();

// Localization (en default, extensible)
builder.Services.AddLocalization();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();

// ProblemDetails (Hellang)
builder.Services.AddProblemDetails(
    options => { options.IncludeExceptionDetails = (ctx, ex) => builder.Environment.IsDevelopment(); });

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

// Seeding
builder.Services.AddHostedService<StartupSeeder>();

// OpenAPI for DTOs/problem schema
builder.Services.AddOpenApi();

var app = builder.Build();

// Middleware: ProblemDetails
app.UseProblemDetails();

// Correlation ID middleware (simple)
app.Use(
    async (context, next) =>
    {
        const string header = "X-Correlation-Id";
        if (!context.Request.Headers.TryGetValue(header, out var id) || string.IsNullOrWhiteSpace(id))
        {
            id = Guid.NewGuid().ToString();
            context.Request.Headers[header] = id;
        }

        context.Response.Headers[header] = id;
        await next();
    });

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