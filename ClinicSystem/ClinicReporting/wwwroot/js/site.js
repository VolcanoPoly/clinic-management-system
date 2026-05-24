(function () {
    'use strict';

    document.querySelectorAll('.needs-validation').forEach(function (form) {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });

    document.querySelectorAll('[asp-validation-summary], .validation-summary-errors').forEach(function (summary) {
        var hasContent = summary.querySelector('li, ul li') || summary.textContent.trim().length > 0;
        if (hasContent) {
            summary.classList.remove('d-none');
            summary.style.display = '';
        }
    });
})();
