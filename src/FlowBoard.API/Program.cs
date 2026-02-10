using Azure.Identity;
using FlowBoard.API.Hubs;
using FlowBoard.API.Middleware;
using FlowBoard.API.Services;
using FlowBoard.Application;
using FlowBoard.Core.Interfaces;
using FlowBoard.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Azure Key Vault integration (production only)
if (!builder.Environment.IsDevelopment())
{
    var keyVaultName = builder.Configuration["KeyVault:Name"];
    if (!string.IsNullOrEmpty(keyVaultName))
    {
        var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
        builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
    }
}

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Azure Application Insights
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddApplicationInsightsTelemetry();
}

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configure SignalR
builder.Services.AddSignalR();

// Register SignalR notification services for real-time updates
builder.Services.AddScoped<IBoardNotificationService, SignalRBoardNotificationService>();
builder.Services.AddScoped<IUserNotificationService, SignalRUserNotificationService>();
builder.Services.AddScoped<ICanvasNotificationService, SignalRCanvasNotificationService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Configure Authentication
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("JWT Key is not configured. Set 'Jwt:Key' in appsettings or Azure Key Vault.")))
        };

        // Allow SignalR to receive JWT token from query string (WebSocket connections can't send headers)
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Configure Memory Cache
builder.Services.AddMemoryCache();

// Health checks with database connectivity
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgresql",
        tags: ["db", "ready"]);

var app = builder.Build();

// Apply pending database schema changes (one-time migration fix)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FlowBoard.Infrastructure.Data.FlowBoardDbContext>();
    try
    {
        // Add TaskId column to Canvases if it doesn't exist
        var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_name = 'Canvases' AND column_name = 'TaskId'
                ) THEN
                    ALTER TABLE ""Canvases"" ADD COLUMN ""TaskId"" INTEGER NULL;
                    CREATE UNIQUE INDEX ""IX_Canvases_TaskId"" ON ""Canvases"" (""TaskId"") WHERE ""TaskId"" IS NOT NULL;
                    ALTER TABLE ""Canvases"" ADD CONSTRAINT ""FK_Canvases_Tasks_TaskId""
                        FOREIGN KEY (""TaskId"") REFERENCES ""Tasks""(""Id"") ON DELETE SET NULL;
                END IF;
            END $$;
        ";
        await command.ExecuteNonQueryAsync();
        Log.Information("Database schema verified/updated for Canvas TaskId");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to apply Canvas TaskId migration (may already exist)");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// CORS must come before HTTPS redirection to handle preflight requests
app.UseCors("AllowFrontend");

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging();

// Serve Angular SPA in production (when deployed together)
if (!app.Environment.IsDevelopment())
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.MapControllers();

// Map SignalR Hubs
app.MapHub<BoardHub>("/hubs/board");
app.MapHub<CanvasHub>("/hubs/canvas");

// Health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false // Liveness: always returns healthy if app is running
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready") // Readiness: checks database connectivity
});

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
