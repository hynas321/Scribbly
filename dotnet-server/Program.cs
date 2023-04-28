using Dotnet.Server.Hubs;
using Dotnet.Server.JsonConfig;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigHelper configHelper = new ConfigHelper();
Config config = configHelper.GetConfig();

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
app.MapHub<ConnectionHub>($"/hub/connection");
app.Run(config?.HttpServerUrl);
