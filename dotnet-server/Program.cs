using System.Text.Json;
using Dotnet.Server.Config;
using Dotnet.Server.Hubs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string json = File.ReadAllText("config.json");
Config? config = JsonSerializer.Deserialize<Config>(json);

builder.Services.AddControllers()
    .AddJsonOptions(options => 
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = new PascalCaseNamingPolicy();
    });

builder.Services.AddSignalR(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => 
{
    options.AddPolicy("AllowReactApp",
        builder => builder.WithOrigins(config?.CorsOrigin!)
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
app.MapHub<LobbyHub>($"/hub/lobby");
app.Run(config?.HttpServerUrl);
