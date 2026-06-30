using FirebaseAdmin.Messaging;
using SmartApart.API.Interfaces.Repositories;
using SmartApart.API.Interfaces.Services;

namespace SmartApart.API.Services
{
    public class FcmService : IFcmService
    {
        private readonly IFcmTokenRepository _fcmTokenRepo;
        private readonly ILogger<FcmService> _logger;

        public FcmService(IFcmTokenRepository fcmTokenRepo, ILogger<FcmService> logger)
        {
            _fcmTokenRepo = fcmTokenRepo;
            _logger = logger;
        }

        // ── Send to all active devices of a user ───────────────────────

        public async Task SendToUserAsync(Guid userId, string title, string body, Dictionary<string, string> data)
        {
            try
            {
                var tokens = await _fcmTokenRepo.GetActiveTokensByUserIdAsync(userId);

                if (tokens.Count == 0)
                {
                    _logger.LogInformation("No active FCM tokens found for user {UserId}.", userId);
                    return;
                }

                var message = new MulticastMessage
                {
                    Tokens = tokens.Select(t => t.Token).ToList(),
                    Notification = new Notification { Title = title, Body = body },
                    Data = data
                };

                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

                await HandleFailedTokensAsync(tokens, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendToUserAsync failed for user {UserId}", userId);
            }
        }

        // ── Send to a single token ──────────────────────────────────────

        public async Task SendToTokenAsync(string fcmToken, string title, string body, Dictionary<string, string> data)
        {
            try
            {
                var message = new Message
                {
                    Token = fcmToken,
                    Notification = new Notification { Title = title, Body = body },
                    Data = data
                };

                await FirebaseMessaging.DefaultInstance.SendAsync(message);
            }
            catch (FirebaseMessagingException ex) when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                _logger.LogWarning("FCM token unregistered: {Token}", fcmToken);

                var existing = await _fcmTokenRepo.GetByTokenAsync(fcmToken);
                if (existing != null)
                {
                    await _fcmTokenRepo.DeactivateAsync(existing);
                    await _fcmTokenRepo.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendToTokenAsync failed for token {Token}", fcmToken);
            }
        }

        // ── Send to a topic (e.g. broadcast announcements) ──────────────

        public async Task SendToTopicAsync(string topic, string title, string body, Dictionary<string, string> data)
        {
            try
            {
                var message = new Message
                {
                    Topic = topic,
                    Notification = new Notification { Title = title, Body = body },
                    Data = data
                };

                await FirebaseMessaging.DefaultInstance.SendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendToTopicAsync failed for topic {Topic}", topic);
            }
        }

        // ── Private: clean up unregistered tokens after multicast send ──

        private async Task HandleFailedTokensAsync(List<Entities.FcmToken> tokens, BatchResponse response)
        {
            for (int i = 0; i < response.Responses.Count; i++)
            {
                var sendResponse = response.Responses[i];

                if (!sendResponse.IsSuccess &&
                    sendResponse.Exception is FirebaseMessagingException fme &&
                    fme.MessagingErrorCode == MessagingErrorCode.Unregistered)
                {
                    _logger.LogWarning("Deactivating unregistered FCM token: {Token}", tokens[i].Token);
                    await _fcmTokenRepo.DeactivateAsync(tokens[i]);
                }
            }

            await _fcmTokenRepo.SaveChangesAsync();
        }
    }

}
