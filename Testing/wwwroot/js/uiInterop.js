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

window.modalInterop = {
    showMyModal: function (modalId) {
        var modalEl = document.getElementById(modalId);
        if (modalEl) {
            var myModal = new bootstrap.Modal(modalEl);
            myModal.show();
        }
    },

    // NEW: buat modal boleh drag
    makeDraggable: function (modalId) {
        const modalDialog = document.querySelector('#' + modalId + ' .modal-dialog');
        const modalHeader = document.querySelector('#' + modalId + ' .modal-header');
        if (!modalDialog || !modalHeader) return;

        // Buang centered class — akan clash dengan position fixed
        modalDialog.classList.remove('modal-dialog-centered');

        const rect = modalDialog.getBoundingClientRect();
        modalDialog.style.position = 'fixed';
        modalDialog.style.margin = '0';
        modalDialog.style.left = rect.left + 'px';
        modalDialog.style.top = rect.top + 'px';
        modalDialog.style.width = rect.width + 'px';
        modalHeader.style.cursor = 'move';

        let isDragging = false;
        let startX, startY, initLeft, initTop;

        modalHeader.addEventListener('mousedown', function (e) {
            if (e.target.closest('button')) return;
            isDragging = true;
            startX = e.clientX;
            startY = e.clientY;
            initLeft = parseFloat(modalDialog.style.left);
            initTop = parseFloat(modalDialog.style.top);
            e.preventDefault();
        });

        document.addEventListener('mousemove', function (e) {
            if (!isDragging) return;
            modalDialog.style.left = (initLeft + e.clientX - startX) + 'px';
            modalDialog.style.top = (initTop + e.clientY - startY) + 'px';
        });

        document.addEventListener('mouseup', function () {
            isDragging = false;
        });
    }
};
