
namespace CuescoreBuddy.Services
{
    public interface IPlayerNotificationService
    {
        Task<List<CuescoreNotification>> ProcessNotifications();
        Task SendNotifications(List<CuescoreNotification> notifications);
    }
}