using WebApi.Hubs;
using WebApi.Api.Hubs;
using WebApi.Application.Managers;
using WebApi.Application.Managers.Interfaces;
using WebApi.Application.Services;
using WebApi.Application.Services.Interfaces;
using WebApi.Infrastructure.Repositories;
using WebApi.Infrastructure.Repositories.Interfaces;
using WebApi.Repositories;
using WebApi.Repositories.Interfaces;
using WebApi.Api.Utilities;
using WebApi.Api.Hubs.Filters;
using Microsoft.AspNetCore.SignalR;
using WebApi.Api.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => 
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = new PascalCaseNamingPolicy();
    });

builder.Services.AddTransient<IHashManager, HashManager>();
builder.Services.AddSingleton<IGameManager, GameManager>();
builder.Services.AddScoped<IPlayerManager, PlayerManager>();
builder.Services.AddScoped<IChatManager, ChatManager>();
builder.Services.AddTransient<IRandomWordService, RandomWordService>();
builder.Services.AddSingleton<IAccountRepository, AccountRepository>();
builder.Services.AddSingleton<IWordRepository, WordRepository>();
builder.Services.AddSingleton<IGameRepository, GameRepository>();

builder.Services.AddSignalR(options =>
{
    options.AddFilter<HubExceptionFilter>();
    options.EnableDetailedErrors = false;
});

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => 
{
    options.AddPolicy("AllowReactApp",
        builder => builder.WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod()   
    );
});

WebApplication app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseCors("AllowReactApp");
app.MapHub<HubConnection>("/hub/connection");
app.MapHub<LongRunningHubConnection>("/long-running-hub/connection");
app.MapHub<AccountHubConnection>("/account-hub/connection");
app.Run();
