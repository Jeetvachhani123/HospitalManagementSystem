// Hospital Management System - Main JavaScript File

const HospitalMS = {
    formatDate: function (date) {
        const options = { year: 'numeric', month: 'long', day: 'numeric' };
        return new Date(date).toLocaleDateString('en-US', options);
    },

    formatTime: function (timeSpan) {
        if (typeof timeSpan === 'string') {
            const parts = timeSpan.split(':');
            const hours = parseInt(parts[0]);
            const minutes = parts[1];
            const ampm = hours >= 12 ? 'PM' : 'AM';
            const displayHours = hours % 12 || 12;
            return `${displayHours}:${minutes} ${ampm}`;
        }
        return timeSpan;
    },

    confirm: function (message, callback) {
        if (confirm(message)) {
            callback();
        }
    },

    setButtonLoading: function (button, loading = true) {
        const $btn = $(button);
        if (loading) {
            $btn.data('original-text', $btn.html());
            $btn.prop('disabled', true);
            $btn.html('<span class="spinner-border spinner-border-sm me-2"></span>Loading...');
        } else {
            $btn.prop('disabled', false);
            $btn.html($btn.data('original-text'));
        }
    },

    validateForm: function (formId) {
        const form = document.getElementById(formId);
        if (form) {
            return form.checkValidity();
        }
        return false;
    }
};

$(function () {
    $('.alert:not(.alert-permanent)').delay(5000).fadeOut('slow');
    $('[data-confirm]').on('click', function (e) {
        const message = $(this).data('confirm');
        if (!confirm(message)) {
            e.preventDefault();
            return false;
        }
    });

    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
});