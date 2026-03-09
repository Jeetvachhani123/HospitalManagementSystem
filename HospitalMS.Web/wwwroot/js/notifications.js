/// <reference path="../lib/microsoft-signalr/dist/browser/signalr.js" />

class NotificationClient {

    constructor() {
        this.connection = null;
        this.currentUserRole = null;
        this.currentUserId = null;
        this.isConnected = false;
    }

    init(userRole, userId) {

        this.currentUserRole = userRole;
        this.currentUserId = userId;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .withAutomaticReconnect()
            .build();

        this.registerConnectionEvents();
        this.registerMessageHandlers();
        this.start();
    }

    start() {
        this.connection.start()
            .then(() => {
                console.log("✅ SignalR connected");
                this.updateConnectionStatus(true);
                this.joinAppropriateGroups();
            })
            .catch(err => {
                console.error("❌ SignalR connection error:", err);
                this.updateConnectionStatus(false);
                setTimeout(() => this.start(), 5000);
            });
    }

    registerConnectionEvents() {

        this.connection.onreconnecting(() => {
            console.log("🔄 Reconnecting...");
            this.updateConnectionStatus(false);
        });

        this.connection.onreconnected(() => {
            console.log("✅ Reconnected");
            this.updateConnectionStatus(true);
            this.joinAppropriateGroups();
        });

        this.connection.onclose(() => {
            console.log("❌ Disconnected");
            this.updateConnectionStatus(false);
        });
    }

    joinAppropriateGroups() {

        if (this.connection.state !== signalR.HubConnectionState.Connected)
            return;

        if (this.currentUserRole === "Doctor") {
            this.connection.invoke("JoinGroup", `doctor_${this.currentUserId}`);
        }
        else if (this.currentUserRole === "Patient") {
            this.connection.invoke("JoinGroup", `patient_${this.currentUserId}`);
        }
        else if (this.currentUserRole === "Admin") {
            this.connection.invoke("JoinGroup", "admins");
            this.connection.invoke("JoinGroup", "doctors");
        }
    }

    registerMessageHandlers() {

        this.connection.on("AppointmentNotification", (notification) => {
            this.handleAppointmentNotification(notification);
        });

        this.connection.on("PendingCountUpdated", (data) => {
            this.handlePendingCountUpdate(data);
        });

        this.connection.on("DashboardUpdated", (update) => {
            this.updateDashboardStats(update);
        });

        this.connection.on("ReceiveNotification", (message) => {
            this.showNotification("Info", message, "info");
        });
    }

    handleAppointmentNotification(notification) {

        console.log("📢 Appointment Notification:", notification);

        let title = "";
        let message = notification.message ?? "New update received.";
        let type = "info";

        switch (notification.type) {

            case "AppointmentRequested":
                title = "📋 New Appointment Request";
                message = `${notification.patientName} requested appointment on ${notification.appointmentDate}`;
                type = "info";
                this.incrementNotificationCount();
                break;

            case "AppointmentApproved":
                title = "✅ Appointment Approved";
                type = "success";
                break;

            case "AppointmentRejected":
                title = "❌ Appointment Rejected";
                type = "danger";
                break;

            case "AppointmentCompleted":
                title = "✔️ Appointment Completed";
                type = "success";
                break;

            case "AppointmentCancelled":
                title = "🚫 Appointment Cancelled";
                type = "warning";
                break;

            case "AppointmentRescheduled":
                title = "🔄 Appointment Rescheduled";
                type = "info";
                break;
        }

        this.showNotification(title, message, type);
        this.playNotificationSound();
    }

    handlePendingCountUpdate(data) {

        const badge = document.getElementById("badge-requests");

        if (badge) {
            badge.textContent = data.count;
            badge.style.display = data.count > 0 ? "inline-block" : "none";
        }
    }

    updateDashboardStats(update) {

        const total = document.getElementById("total-appointments");
        const today = document.getElementById("today-appointments");
        const pending = document.getElementById("pending-approvals");

        if (total) total.textContent = update.totalAppointments;
        if (today) today.textContent = update.todayAppointments;
        if (pending) pending.textContent = update.pendingApprovals;
    }

    incrementNotificationCount() {
        const el = document.getElementById("notification-count");
        if (el) {
            let count = parseInt(el.textContent || "0");
            el.textContent = count + 1;
        }
    }

    showNotification(title, message, type = "info") {

        const toastType = type === "danger" ? "danger" : type;

        if (typeof showToast === "function") {
            showToast(`<strong>${title}</strong><br/>${message}`, toastType);
        }
        else {
            console.log(`[${type}] ${title}: ${message}`);
        }
    }

    playNotificationSound() {
        try {
            const audio = new Audio("/sounds/notification.mp3");
            audio.play();
        } catch {
            // Ignore sound errors
        }
    }

    updateConnectionStatus(connected) {

        this.isConnected = connected;
        const indicator = document.getElementById("connection-indicator");

        if (indicator) {
            indicator.className = connected
                ? "connection-indicator connected"
                : "connection-indicator disconnected";
        }
    }

    stop() {
        if (this.connection) {
            this.connection.stop();
        }
    }
}

// Global Instance
let notificationClient = null;

document.addEventListener("DOMContentLoaded", function () {

    const roleElement = document.getElementById("current-user-role");
    const idElement = document.getElementById("current-user-id");

    if (roleElement && idElement) {

        notificationClient = new NotificationClient();
        notificationClient.init(roleElement.value, idElement.value);
    }
});

window.addEventListener("beforeunload", function () {
    if (notificationClient) {
        notificationClient.stop();
    }
});
