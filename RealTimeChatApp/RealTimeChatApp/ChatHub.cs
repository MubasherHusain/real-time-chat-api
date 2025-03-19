using Microsoft.AspNetCore.SignalR;
using RealTimeChatApp.Models;
using RealTimeChatApp.Repository;

namespace RealTimeChatApp;

public class ChatHub : Hub
{
    private readonly IMessageRepository _userRepository;

    public ChatHub(IMessageRepository userRepository)
    {
        _userRepository = userRepository;
    }
    public async Task SendMessage(int senderId, int receiverId, string message, string senderUsername)
    {
        try
        {
            senderUsername ??= await _userRepository.GetUsernameById(senderId);
            var chatMessage = new
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message,
                SenderUsername = senderUsername,
                SentAt = DateTime.UtcNow
            };
            await _userRepository.SaveMessageAsync(new MessageDto
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message
            });

            // Send message to both sender and receiver in their groups
            await Clients.Group(senderId.ToString()).SendAsync("ReceiveMessage", chatMessage);
            await Clients.Group(receiverId.ToString()).SendAsync("ReceiveMessage", chatMessage);
            await NotifyMessageReceived(senderId, receiverId);
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error in SendMessage: {ex.Message}");
            throw; // Re-throw the exception to propagate it to the client
        }


    }
    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"];
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"User {userId} joined group.");
        }
        await base.OnConnectedAsync();
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"];
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"User {userId} left group.");
        }
        await base.OnDisconnectedAsync(exception);
    }
    public async Task NotifyMessageReceived(int senderId, int receiverId)
    {
        var messages = await _userRepository.GetUndeliveredMessages(receiverId);
        foreach (var message in messages)
        {
            await _userRepository.MarkAsDeliveredAsync(message.Id);
        }
        await Clients.Group(receiverId.ToString()).SendAsync("UpdateChatHistory");
        await Clients.Group(senderId.ToString()).SendAsync("UpdateChatHistory");
    }
}
