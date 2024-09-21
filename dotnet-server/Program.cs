using Dotnet.Server.Hubs;
using dotnet_server.Api.Hubs;
using dotnet_server.Application.Managers;
using dotnet_server.Application.Managers.Interfaces;
using dotnet_server.Application.Services;
using dotnet_server.Application.Services.Interfaces;
using dotnet_server.Infrastructure.Repositories;
using dotnet_server.Infrastructure.Repositories.Interfaces;
using dotnet_server.Repositories;
using dotnet_server.Repositories.Interfaces;

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
builder.Services.AddSignalR(); 
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
