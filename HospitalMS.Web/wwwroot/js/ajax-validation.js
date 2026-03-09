// AJAX Form Validation and Submission Handler
(function () {
    'use strict';
    window.initializeAjaxForms = function () {
        const forms = document.querySelectorAll('[data-ajax="true"]');
        forms.forEach(form => {
            form.addEventListener('submit', handleFormSubmit);
        });
    };

    async function handleFormSubmit(e) {
        e.preventDefault();
        const form = e.target;
        const submitButton = form.querySelector('button[type="submit"]');
        const originalButtonText = submitButton.textContent;
        try {
            submitButton.disabled = true;
            submitButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Loading...';
            if (!form.checkValidity() === false) {
                e.stopPropagation();
                return;
            }
            clearFieldErrors(form);
            const formData = new FormData(form);
            const action = form.getAttribute('action');
            const method = form.getAttribute('method') || 'POST';
            const response = await fetch(action, {
                method: method.toUpperCase(),
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
            if (response.ok) {
                const redirectUrl = response.headers.get('X-Redirect-Url');
                if (redirectUrl) {
                    window.location.href = redirectUrl;
                } else if (form.hasAttribute('data-success-action')) {
                    window.location.href = form.getAttribute('data-success-action');
                } else {
                    showSuccessMessage('Operation completed successfully!');
                    form.reset();
                    setTimeout(() => window.location.reload(), 1500);
                }
            } else if (response.status === 400) {
                const errors = await response.json();
                displayFieldErrors(form, errors);
                showErrorMessage('Please correct the errors below.');
            } else {
                showErrorMessage('An error occurred. Please try again.');
            }
        } catch (error) {
            console.error('Form submission error:', error);
            showErrorMessage('An unexpected error occurred. Please try again.');
        } finally {
            submitButton.disabled = false;
            submitButton.textContent = originalButtonText;
        }
    }

    function clearFieldErrors(form) {
        form.querySelectorAll('.field-error').forEach(el => {
            el.classList.remove('field-error');
            el.classList.remove('is-invalid');
        });
        form.querySelectorAll('.error-message').forEach(el => {
            el.remove();
        });
    }

    function displayFieldErrors(form, errors) {
        clearFieldErrors(form);
        Object.keys(errors).forEach(fieldName => {
            const field = form.querySelector(`[name="${fieldName}"]`);
            if (field) {
                field.classList.add('is-invalid', 'field-error');
                const errorDiv = document.createElement('div');
                errorDiv.className = 'invalid-feedback error-message d-block';
                errorDiv.textContent = errors[fieldName][0] || 'Invalid input';
                field.parentNode.appendChild(errorDiv);
            }
        });
    }

    function showSuccessMessage(message) {
        const alert = document.createElement('div');
        alert.className = 'alert alert-success alert-dismissible fade show';
        alert.setAttribute('role', 'alert');
        alert.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        document.body.insertBefore(alert, document.body.firstChild);
        setTimeout(() => alert.remove(), 5000);
    }

    function showErrorMessage(message) {
        const alert = document.createElement('div');
        alert.className = 'alert alert-danger alert-dismissible fade show';
        alert.setAttribute('role', 'alert');
        alert.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        document.body.insertBefore(alert, document.body.firstChild);
        setTimeout(() => alert.remove(), 5000);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeAjaxForms);
    } else {
        initializeAjaxForms();
    }
})();