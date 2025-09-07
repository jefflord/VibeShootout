using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace VibeShootout.Backend.Hubs
{
    public class CodeReviewHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            Console.WriteLine($"SignalR: Client {Context.ConnectionId} joining group {groupName}");
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            Console.WriteLine($"SignalR: Client {Context.ConnectionId} leaving group {groupName}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"SignalR: Client connected - {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"SignalR: Client disconnected - {Context.ConnectionId}. Exception: {exception?.Message}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}