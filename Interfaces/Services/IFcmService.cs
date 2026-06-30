namespace SmartApart.API.Interfaces.Services
{
    public interface IFcmService
    {
        Task SendToUserAsync(Guid userId, string title, string body, Dictionary<string, string> data);
        Task SendToTokenAsync(string fcmToken, string title, string body, Dictionary<string, string> data);
        Task SendToTopicAsync(string topic, string title, string body, Dictionary<string, string> data);

    }
}
