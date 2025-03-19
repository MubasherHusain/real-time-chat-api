using Npgsql;
using Dapper;
using RealTimeChatApp.Models;

namespace RealTimeChatApp.Repository;

public class MessageRepository : IMessageRepository
{
    private readonly NpgsqlConnection _db;

    public MessageRepository(NpgsqlConnection db) => _db = db;

    public async Task SaveMessageAsync(MessageDto message)
    {
        var query = @"INSERT INTO messages (sender_id, receiver_id, content, is_delivered, is_read, sent_at) 
                  VALUES (@SenderId, @ReceiverId, @Content, FALSE, FALSE, NOW());";

        await _db.ExecuteAsync(query, message);
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesBetweenUsers(int user1, int user2)
    {
        var query = @"SELECT m.id AS Id, 
                     m.sender_id AS SenderId, 
                     m.receiver_id AS ReceiverId,
                     sender.username AS SenderUsername, 
                     u.username AS ReceiverUsername, 
                     m.content AS Content, 
                     m.is_delivered AS IsDelivered, 
                     m.is_read AS IsRead, 
                     m.sent_at AS SentAt, 
                     m.delivered_at AS DeliveredAt, 
                     m.read_at AS ReadAt 
              FROM messages m
 JOIN users sender ON m.sender_id = sender.id
              JOIN users u ON m.receiver_id = u.id
              WHERE (m.sender_id = @user1 AND m.receiver_id = @user2) 
                 OR (m.sender_id = @user2 AND m.receiver_id = @user1)
              ORDER BY m.sent_at ASC;";
        return await _db.QueryAsync<MessageDto>(query, new { user1, user2 });
    }
    public async Task MarkAsDeliveredAsync(int messageId)
    {
        var query = @"UPDATE messages 
                  SET is_delivered = TRUE, delivered_at = NOW() 
                  WHERE id = @MessageId AND is_delivered = FALSE;";
        await _db.ExecuteAsync(query, new { MessageId = messageId });
    }
    public async Task<IEnumerable<MessageDto>> GetUndeliveredMessages(int receiverId)
    {
        var query = @"SELECT * FROM messages WHERE receiver_id = @ReceiverId AND is_delivered = FALSE";
        return await _db.QueryAsync<MessageDto>(query, new { ReceiverId = receiverId });
    }
    public async Task MarkAsReadAsync(int messageId)
    {
        var query = @"UPDATE messages 
                  SET is_read = TRUE, read_at = NOW() 
                  WHERE id = @MessageId AND is_read = FALSE;";
        await _db.ExecuteAsync(query, new { MessageId = messageId });
    }
    public async Task<IEnumerable<UserDto>> GetUsers()
    {
        var query = @"SELECT id, username FROM users";
        return await _db.QueryAsync<UserDto>(query);
    }
    public async Task<UserDto?> GetUserByUsername(string userName)
{
    var query = @"SELECT id AS Id, username AS UserName FROM users WHERE username = @UserName";

    return await _db.QueryFirstOrDefaultAsync<UserDto>(query, new { UserName = userName });
}
    public async Task<string?> GetUsernameById(int userId)
    {
        var query = @"SELECT username FROM users WHERE id = @UserId LIMIT 1;";
        return await _db.QueryFirstOrDefaultAsync<string>(query, new { UserId = userId });
    }
}
