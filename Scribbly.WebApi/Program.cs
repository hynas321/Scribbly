using Scribbly.WebApi.Extensions;
using Scribbly.WebApi.Middleware;
using Scribbly.WebApi.Hubs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddScribblyBackend(builder.Configuration, builder.Environment);
WebApplication app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
app.UseCors(ServiceCollectionExtensions.ReactAppCorsPolicy);
app.MapControllers();
app.MapHub<HubConnection>("/hub/connection");
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = registration => registration.Tags.Contains("ready") });
app.Run();
public partial class Program
{
}
