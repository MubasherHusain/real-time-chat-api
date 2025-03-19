using RealTimeChatApp.Models;
using RealTimeChatApp.Repository;

namespace RealTimeChatApp.Service;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepo;

    public MessageService(IMessageRepository messageRepo) => _messageRepo = messageRepo;

    public async Task SaveMessageAsync(MessageDto message) =>
        await _messageRepo.SaveMessageAsync(message);

    public async Task<IEnumerable<MessageDto>> GetChatHistory(int user1, int user2) =>
        await _messageRepo.GetMessagesBetweenUsers(user1, user2);
    public async Task<UserDto?> LoginAsync(string userName)
    {
        return await _messageRepo.GetUserByUsername(userName);
    }
    public async Task<IEnumerable<UserDto>> GetUsers()
    {
        return await _messageRepo.GetUsers();
    }
}
