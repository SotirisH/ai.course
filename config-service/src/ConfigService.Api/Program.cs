using ConfigService.Infrastructure.Data;
using ConfigService.Infrastructure.Repositories;
using ConfigService.Application.Interfaces;
using ConfigService.Application.Services;
using ConfigService.Api.Middleware;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Serilog;
using dotenv.net;

// Load .env file
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Override configuration with environment variables
builder.Configuration.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();

// Configure FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<ConfigService.Application.Validators.CreateApplicationRequestValidator>();

// Configure database
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ConfigDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register repositories
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();

// Register services
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection(); 
    
    
    // Auto-migrate database in development
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ConfigDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Use custom exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
 var I=
//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
