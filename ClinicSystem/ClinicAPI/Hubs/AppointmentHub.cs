using Microsoft.AspNetCore.SignalR;

namespace ClinicAPI.Hubs;

// ⚠ STUB — This file will be fully replaced in Step 30.
// It exists now so the solution compiles.
public class AppointmentHub : Hub
{
    /// <summary>
    /// Clients call this to subscribe to the live waiting room feed.
    /// </summary>
    public async Task JoinWaitingRoom()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "WaitingRoom");
    }

    /// <summary>
    /// Clients call this to stop receiving waiting room updates.
    /// </summary>
    public async Task LeaveWaitingRoom()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "WaitingRoom");
    }
}