using Ai.Api.Application;
using Ai.Api.Infrastructure;
using Ai.Api.Infrastructure.Persistence.Context;
using Ai.Api.Middleware;
using Scalar.AspNetCore;
using Wolverine;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

WebApplication app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapOpenApi();
app.MapScalarApiReference();

app.Run();
