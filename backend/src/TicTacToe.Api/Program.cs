using System.Text.Json.Serialization;
using TicTacToe.Api;
using TicTacToe.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IComputerPlayer, ComputerPlayer>();
builder.Services.AddSingleton<IGameSessionService, GameSessionService>();
builder.Services.AddExceptionHandler<GameExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseCors();
app.MapControllers();
app.Run();

public partial class Program;
