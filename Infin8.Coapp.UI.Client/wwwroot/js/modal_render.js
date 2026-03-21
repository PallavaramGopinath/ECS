// Modal helper functions
window.modalHelpers = {
    showModal: function (modalId) {
        var modalElement = document.getElementById(modalId);
        if (modalElement) {
            var modal = new bootstrap.Modal(modalElement);
            modal.show();
            return modal;
        }
    },
    hideModal: function (modalId) {
        var modalElement = document.getElementById(modalId);
        if (modalElement) {
            var modal = bootstrap.Modal.getInstance(modalElement);
            if (modal) {
                modal.hide();
            }
        }
    }
};