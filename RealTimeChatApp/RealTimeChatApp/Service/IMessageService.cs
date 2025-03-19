using RealTimeChatApp.Models;

public interface IMessageService
{
    Task SaveMessageAsync(MessageDto message);
    Task<IEnumerable<MessageDto>> GetChatHistory(int user1, int user2);
    Task<IEnumerable<UserDto>> GetUsers();
    Task<UserDto?> LoginAsync(string username);
}