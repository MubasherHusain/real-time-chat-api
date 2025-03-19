using RealTimeChatApp.Models;

namespace RealTimeChatApp.Repository;

public interface IMessageRepository
{
    Task SaveMessageAsync(MessageDto message);
    Task<IEnumerable<MessageDto>> GetMessagesBetweenUsers(int user1, int user2);
    Task<IEnumerable<UserDto>> GetUsers();
    Task<UserDto?> GetUserByUsername(string username);
    Task<string?> GetUsernameById(int userId);
    Task<IEnumerable<MessageDto>> GetUndeliveredMessages(int receiverId);
    Task MarkAsDeliveredAsync(int messageId);

    }
