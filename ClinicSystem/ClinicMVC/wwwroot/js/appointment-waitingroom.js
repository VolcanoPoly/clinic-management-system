"use strict";

(function () {
    const pagePath = window.location.pathname.toLowerCase();
    if (!pagePath.includes("/appointment/todaysappointments")) {
        return;
    }

    const statusClasses = {
        Requested: "badge bg-secondary",
        Confirmed: "badge bg-info text-dark",
        CheckedIn: "badge bg-primary",
        InProgress: "badge bg-warning text-dark",
        Completed: "badge bg-success",
        Cancelled: "badge bg-danger",
        Missed: "badge bg-dark"
    };

    function renderStatusBadge(status) {
        const cssClass = statusClasses[status] || "badge bg-secondary";
        return `<span class="${cssClass}">${status}</span>`;
    }

    function showRefreshAlert(message) {
        const messageBox = document.getElementById("waiting-room-message");
        if (!messageBox) return;

        messageBox.innerHTML = `<div class="alert alert-success alert-sm mb-0">${message}</div>`;
        messageBox.classList.remove("d-none");

        window.clearTimeout(window.waitingRoomAlertTimer);
        window.waitingRoomAlertTimer = window.setTimeout(() => {
            messageBox.classList.add("d-none");
            messageBox.innerHTML = "";
        }, 6000);
    }

    function updateAppointmentRow(appointment) {
        const row = document.querySelector(`tr[data-appointment-id="${appointment.id}"]`);
        if (!row) return;

        const statusCell = row.querySelector("td.status-cell");
        if (statusCell) {
            statusCell.innerHTML = renderStatusBadge(appointment.status);
        }

        showRefreshAlert(`Appointment at ${new Date(appointment.appointmentDateTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} updated to ${appointment.status}.`);
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/appointment")
        .withAutomaticReconnect()
        .build();

    connection.on("AppointmentStatusChanged", updateAppointmentRow);

    connection.start()
        .then(() => connection.invoke("JoinWaitingRoom"))
        .catch(error => {
            console.error("SignalR connection failed:", error);
        });
})();
