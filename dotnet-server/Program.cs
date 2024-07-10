using Dotnet.Server.Database;
using Dotnet.Server.Hubs;
using Dotnet.Server.Managers;
using dotnet_server.Repositories;
using dotnet_server.Repositories.Interfaces;
using dotnet_server.Services;
using dotnet_server.Services.Interfaces;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => 
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = new PascalCaseNamingPolicy();
    });

builder.Services.AddTransient<IRandomWordService, RandomWordService>();
builder.Services.AddSingleton<IAccountRepository, AccountRepository>();
builder.Services.AddSingleton<IWordRepository, WordRepository>();
builder.Services.AddSingleton<IGameRepository, GameRepository>();
builder.Services.AddSingleton<IGameManager, GameManager>();
builder.Services.AddTransient<IHashManager, HashManager>();
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

// Configure the HTTP request pipeline.
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
