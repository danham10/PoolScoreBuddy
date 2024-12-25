
namespace PoolScoreBuddy.Services
{
    public interface IPlayerNotificationService
    {
        Task<List<PlayerNotification>> ProcessNotifications();
        Task SendNotifications(List<PlayerNotification> notifications);
    }
}