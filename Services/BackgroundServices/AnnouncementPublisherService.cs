using SmartApart.API.Interfaces.Repositories;
using SmartApart.API.Interfaces.Services;

namespace SmartApart.API.Services.BackgroundServices
{
    public class AnnouncementPublisherService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AnnouncementPublisherService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(60);

        public AnnouncementPublisherService(
            IServiceScopeFactory scopeFactory,
            ILogger<AnnouncementPublisherService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Announcement Publisher Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PublishDueAnnouncementsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in AnnouncementPublisherService.");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Announcement Publisher Background Service stopped.");
        }

        private async Task PublishDueAnnouncementsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var announcementRepo = scope.ServiceProvider.GetRequiredService<IAnnouncementRepository>();
            var fcmService = scope.ServiceProvider.GetRequiredService<IFcmService>();

            var dueAnnouncements = await announcementRepo.GetDueForPublishingAsync();
            if (dueAnnouncements.Count == 0)
                return;

            _logger.LogInformation("Found {Count} announcement(s) due for publishing.", dueAnnouncements.Count);

            foreach (var announcement in dueAnnouncements)
            {
                announcement.IsPublished = true;
                announcement.PublishedAt = DateTime.UtcNow;
                announcement.UpdatedAt = DateTime.UtcNow;
                await announcementRepo.UpdateAsync(announcement);

                var data = new Dictionary<string, string>
                {
                    { "type", "Announcement" },
                    { "referenceId", announcement.Id.ToString() }
                };

                await fcmService.SendToTopicAsync("all", announcement.Title, announcement.Body, data);

                _logger.LogInformation("Announcement {Id} — '{Title}' auto-published.", announcement.Id, announcement.Title);
            }

            await announcementRepo.SaveChangesAsync();
        }
    }
}
