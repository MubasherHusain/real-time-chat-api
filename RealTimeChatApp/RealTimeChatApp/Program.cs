using Microsoft.AspNetCore.SignalR;
using Npgsql;
using RealTimeChatApp;
using RealTimeChatApp.Models;
using RealTimeChatApp.Repository;
using RealTimeChatApp.Service;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<NpgsqlConnection>(_ => new NpgsqlConnection(connectionString));
builder.Services.AddSignalR();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IMessageService, MessageService>();
var allowedOrigins = "_allowedOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowedOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(allowedOrigins);
app.UseHttpsRedirection();
app.MapHub<ChatHub>("/chatHub");
app.MapGet("/", () => "Chat API Running...");
app.MapGet("/users", async (IMessageService messageService) =>
{
    var users = await messageService.GetUsers();
    return Results.Ok(users);
});
app.MapPost("/login", async (string userName, IMessageService messageService) =>
{
    if (string.IsNullOrWhiteSpace(userName))
    {
        return Results.BadRequest("Username is required.");
    }

    var user = await messageService.LoginAsync(userName.Trim());

    if (user == null)
    {
        return Results.NotFound("User not found.");
    }

    return Results.Ok(new { user.Id });
});
app.MapPost("/send-message", async (MessageDto message, IMessageService messageService, IHubContext<ChatHub> hubContext) =>
{
    await messageService.SaveMessageAsync(message);
    await hubContext.Clients.User(message.ReceiverId.ToString()).SendAsync("ReceiveMessage", message.SenderId, message.Content);
    return Results.Ok("Message sent!");
});

app.MapGet("/chat-history/{user1}/{user2}", async (int user1, int user2, IMessageService messageService) =>
{
    var messages = await messageService.GetChatHistory(user1, user2);
    return Results.Ok(messages);
});
app.Run();