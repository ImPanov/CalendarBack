using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using CalendarBack.Data;
using CalendarBack.Hubs;

namespace CalendarBack.Services;

public class ReminderService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<ReminderService> _logger;

    public ReminderService(
        IServiceProvider services,
        IHubContext<NotificationHub> hubContext,
        ILogger<ReminderService> logger)
    {
        _services = services;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<CalendarDbContext>();

                try
                {
                    var now = DateTime.UtcNow;
                    var dueReminders = await dbContext.CalendarEntries
                        .Where(e => e.ReminderDateTime <= now)
                        .ToListAsync(stoppingToken);

                    foreach (var reminder in dueReminders)
                    {
                        await _hubContext.Clients.All.SendAsync(
                            "ReceiveNotification",
                            reminder.Title,
                            reminder.Description ?? string.Empty,
                            stoppingToken);

                        _logger.LogInformation(
                            "Sent notification for reminder {Title} scheduled at {DateTime}",
                            reminder.Title,
                            reminder.ReminderDateTime);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing reminders");
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
