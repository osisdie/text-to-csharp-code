using TextToCode.Application.DependencyInjection;
using TextToCode.Infrastructure.DependencyInjection;
using TextToCode.WebApi.Hubs;
using TextToCode.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TextToCode API", Version = "v1" });
});
builder.Services.AddSignalR();

builder.Services.AddTextToCodeApplication(builder.Configuration);
builder.Services.AddTextToCodeInfrastructure(builder.Configuration);

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHub<CodeGenerationHub>("/hubs/codegen");

app.Run();
