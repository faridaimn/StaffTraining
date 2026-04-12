// c:\Users\Acer\source\repos\Testing\Testing\wwwroot\js\uiInterop.js

window.modalInterop = {
    showMyModal: function (modalId) {
        var modalEl = document.getElementById(modalId);
        if (modalEl) {
            var myModal = new bootstrap.Modal(modalEl);
            myModal.show();
        }
    }
};
