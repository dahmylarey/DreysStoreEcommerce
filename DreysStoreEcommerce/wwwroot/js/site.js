
<script>
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .build();

    connection.on("ReceiveNotification", (message, createdAt) => {
        showToast(`${message} (${new Date(createdAt).toLocaleString()})`);
    });

    connection.start().catch(err => console.error(err));

    function showToast(message) {
        const toast = document.createElement("div");
        toast.className = "toast align-items-center text-bg-primary border-0 show";
        toast.style.position = "fixed";
        toast.style.top = "1rem";
        toast.style.right = "1rem";
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" onclick="this.parentElement.parentElement.remove()"></button>
            </div>`;
        document.body.appendChild(toast);
        setTimeout(() => toast.remove(), 5000);
    }
</script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.0/signalr.min.js"></script>
<script>
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .build();

    connection.on("ReceiveNotification", (message, createdAt) => {
        showToast(`${message} (${new Date(createdAt).toLocaleString()})`);
    });

    connection.start().catch(err => console.error(err));

    function showToast(message) {
        const toast = document.createElement("div");
        toast.className = "toast align-items-center text-bg-primary border-0 show";
        toast.style.position = "fixed";
        toast.style.top = "1rem";
        toast.style.right = "1rem";
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" onclick="this.parentElement.parentElement.remove()"></button>
            </div>`;
        document.body.appendChild(toast);
        setTimeout(() => toast.remove(), 5000);
    }
</script>
