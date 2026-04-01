window.showToast = (title, message, type) => {
    // You can use any toast library or custom implementation
    // Here's a simple alert for demonstration
    alert(`${title}: ${message}`);

    // For better UI, consider using Toastify or Bootstrap Toasts
    // Example with Bootstrap Toast:
    
    const toastHTML = `
        <div class="toast align-items-center text-white bg-${type} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${title}</strong><br>${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;
    document.getElementById('toast-container').insertAdjacentHTML('beforeend', toastHTML);
    const toastElement = document.querySelector('.toast:last-child');
    const toast = new bootstrap.Toast(toastElement);
    toast.show();
    
};