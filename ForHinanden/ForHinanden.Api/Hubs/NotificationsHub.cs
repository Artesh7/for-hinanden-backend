using Microsoft.AspNetCore.SignalR;
using ForHinanden.Api.Services;

namespace ForHinanden.Api.Hubs;

public class NotificationsHub : Hub
{
    private readonly ActiveChatRegistry _registry;
    public NotificationsHub(ActiveChatRegistry registry)
    {
        _registry = registry;
    }

    // Client calls when user opens a chat
    public Task JoinChat(string deviceId, string peerId, Guid taskId)
    {
        _registry.JoinChat(deviceId, peerId, taskId, Context.ConnectionId);
        return Task.CompletedTask;
    }

    // Client calls when user leaves a chat
    public Task LeaveChat(string deviceId)
    {
        _registry.LeaveChat(deviceId);
        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _registry.OnDisconnected(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}