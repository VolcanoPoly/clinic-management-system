using Microsoft.AspNetCore.SignalR;

namespace ClinicMVC.Hubs;

public class AppointmentHub : Hub
{
    public async Task JoinWaitingRoom()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "WaitingRoom");
    }

    public async Task LeaveWaitingRoom()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "WaitingRoom");
    }
}
