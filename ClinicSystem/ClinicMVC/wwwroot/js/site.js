(function () {
    'use strict';

    // Bootstrap 5 native validation for forms marked needs-validation
    document.querySelectorAll('.needs-validation').forEach(function (form) {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });

    // Show server-side validation summaries when they contain errors
    document.querySelectorAll('[asp-validation-summary], .validation-summary-errors').forEach(function (summary) {
        var hasContent = summary.querySelector('li, ul li') || summary.textContent.trim().length > 0;
        if (hasContent) {
            summary.classList.remove('d-none');
            summary.style.display = '';
        }
    });

    // Confirmation before destructive actions (data-confirm on forms or submit buttons)
    document.querySelectorAll('[data-confirm]').forEach(function (el) {
        var message = el.getAttribute('data-confirm');
        if (!message) return;

        if (el.tagName === 'FORM') {
            el.addEventListener('submit', function (e) {
                if (!window.confirm(message)) {
                    e.preventDefault();
                    e.stopImmediatePropagation();
                }
            });
            return;
        }

        el.addEventListener('click', function (e) {
            if (!window.confirm(message)) {
                e.preventDefault();
                e.stopImmediatePropagation();
            }
        });
    });

    // Booking step: require a time slot before continuing
    var slotForm = document.getElementById('appointment-slot-form');
    if (slotForm) {
        slotForm.addEventListener('submit', function (e) {
            var selected = slotForm.querySelector('input[name="SelectedSlot"]:checked');
            var feedback = document.getElementById('slotValidationFeedback');
            if (!selected) {
                e.preventDefault();
                e.stopPropagation();
                if (feedback) {
                    feedback.textContent = 'Please select a time slot.';
                    feedback.classList.remove('d-none');
                }
                return;
            }
            if (feedback) feedback.classList.add('d-none');
        });
    }

    // Prescription: ensure at least one medication line has a name
    var rxForm = document.getElementById('prescription-form');
    if (rxForm) {
        rxForm.addEventListener('submit', function (e) {
            var names = rxForm.querySelectorAll('input[name*="MedicationName"]');
            var hasMedication = Array.prototype.some.call(names, function (input) {
                return input.value && input.value.trim().length > 0;
            });
            var feedback = document.getElementById('prescriptionValidationFeedback');
            if (!hasMedication) {
                e.preventDefault();
                e.stopPropagation();
                if (feedback) {
                    feedback.textContent = 'Add at least one medication with a name.';
                    feedback.classList.remove('d-none');
                }
            } else if (feedback) {
                feedback.classList.add('d-none');
            }
        });
    }
})();
