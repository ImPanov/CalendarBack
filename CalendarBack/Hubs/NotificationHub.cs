using Microsoft.AspNetCore.SignalR;

namespace CalendarBack.Hubs;

public class NotificationHub : Hub
{
    public async Task SendNotification(string title, string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", title, message);
    }
}
