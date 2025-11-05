using System;
using System.Collections.Concurrent;

namespace ForHinanden.Api.Services;

public class ActiveChatRegistry
{
    private readonly ConcurrentDictionary<string, (string peerId, Guid taskId)> _deviceToChat = new();
    private readonly ConcurrentDictionary<string, string> _connectionToDevice = new();

    public void JoinChat(string deviceId, string peerId, Guid taskId, string connectionId)
    {
        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(peerId)) return;
        _deviceToChat[deviceId] = (peerId.Trim(), taskId);
        _connectionToDevice[connectionId] = deviceId.Trim();
    }

    public void LeaveChat(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId)) return;
        _deviceToChat.TryRemove(deviceId.Trim(), out _);
    }

    public void OnDisconnected(string connectionId)
    {
        if (_connectionToDevice.TryRemove(connectionId, out var deviceId))
        {
            // Best effort cleanup
            _deviceToChat.TryRemove(deviceId, out _);
        }
    }

    public bool IsInChatWith(string receiverDeviceId, string peerId, Guid taskId)
    {
        if (string.IsNullOrWhiteSpace(receiverDeviceId) || string.IsNullOrWhiteSpace(peerId)) return false;
        if (_deviceToChat.TryGetValue(receiverDeviceId.Trim(), out var entry))
        {
            var peerMatch = string.Equals(entry.peerId, peerId.Trim(), StringComparison.OrdinalIgnoreCase);
            var taskMatch = entry.taskId == Guid.Empty || taskId == Guid.Empty || entry.taskId == taskId;
            return peerMatch && taskMatch;
        }
        return false;
    }
}

