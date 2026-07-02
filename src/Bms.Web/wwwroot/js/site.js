// Business Management System Core JavaScript Module
document.addEventListener("DOMContentLoaded", () => {
    // Enable sidebar toggling on mobile
    const toggleBtn = document.getElementById("sidebarToggle");
    const sidebar = document.querySelector(".bms-sidebar");
    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener("click", () => {
            sidebar.classList.toggle("active");
        });
    }



    // Auto-initialize standard tooltips/popovers
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map((tooltipTriggerEl) => new bootstrap.Tooltip(tooltipTriggerEl));
});

// CSRF Anti-Forgery AJAX Wrapper
async function bmsAjax(url, method = "GET", data = null) {
    const headers = {
        "X-Requested-With": "XMLHttpRequest"
    };

    // Extract Anti-Forgery Token
    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value || 
                       document.querySelector('meta[name="csrf-token"]')?.getAttribute("content");

    if (csrfToken) {
        headers["RequestVerificationToken"] = csrfToken;
    }

    const options = {
        method: method,
        headers: headers
    };

    if (data) {
        if (data instanceof FormData) {
            options.body = data;
        } else {
            headers["Content-Type"] = "application/json";
            options.body = JSON.stringify(data);
        }
    }

    try {
        const response = await fetch(url, options);
        const contentType = response.headers.get("content-type");
        
        if (contentType && contentType.includes("application/json")) {
            const jsonResult = await response.json();
            if (!response.ok) {
                throw new Error(jsonResult.message || `HTTP error! status: ${response.status}`);
            }
            return jsonResult;
        } else {
            const textResult = await response.text();
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return textResult; // Returns raw HTML, e.g., for modal templates
        }
    } catch (error) {
        console.error("AJAX Request Failed:", error);
        bmsAlert(error.message || "An unexpected network error occurred.", "danger");
        throw error;
    }
}

// Floating Toast Notification System
function bmsAlert(message, type = "success", duration = 4000) {
    let alertContainer = document.getElementById("bmsAlertContainer");
    if (!alertContainer) {
        alertContainer = document.createElement("div");
        alertContainer.id = "bmsAlertContainer";
        alertContainer.style.position = "fixed";
        alertContainer.style.top = "1.5rem";
        alertContainer.style.right = "1.5rem";
        alertContainer.style.zIndex = "9999";
        alertContainer.style.display = "flex";
        alertContainer.style.flexDirection = "column";
        alertContainer.style.gap = "0.5rem";
        document.body.appendChild(alertContainer);
    }

    const alertEl = document.createElement("div");
    alertEl.className = `alert alert-${type} alert-dismissible fade show glass-card`;
    alertEl.style.margin = "0";
    alertEl.style.minWidth = "280px";
    alertEl.style.maxWidth = "400px";
    alertEl.style.boxShadow = "0 10px 15px -3px rgba(0, 0, 0, 0.4)";
    alertEl.innerHTML = `
        <div class="d-flex align-items-center gap-2">
            <i class="bi ${type === 'success' ? 'bi-check-circle-fill' : type === 'danger' ? 'bi-exclamation-triangle-fill' : 'bi-info-circle-fill'}"></i>
            <div>${message}</div>
        </div>
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close" style="filter: invert(1);"></button>
    `;

    alertContainer.appendChild(alertEl);

    // Auto-remove after duration
    setTimeout(() => {
        const bsAlert = new bootstrap.Alert(alertEl);
        bsAlert.close();
    }, duration);
}

// Dynamic AJAX Modal CRUD Orchestrator
class BmsModalManager {
    constructor(modalElementId, placeholderContainerId) {
        this.modalId = modalElementId;
        this.containerId = placeholderContainerId;
    }

    async open(url, title = "Form Dialog") {
        try {
            // Fetch HTML partial view from controller
            const htmlContent = await bmsAjax(url, "GET");
            
            let container = document.getElementById(this.containerId);
            if (!container) {
                container = document.createElement("div");
                container.id = this.containerId;
                document.body.appendChild(container);
            }
            container.innerHTML = htmlContent;

            // Initialize and show modal
            const modalEl = document.getElementById(this.modalId);
            if (!modalEl) {
                throw new Error(`Modal element with ID '${this.modalId}' not found in partial view.`);
            }

            // Set modal title
            const titleEl = modalEl.querySelector(".modal-title");
            if (titleEl) titleEl.textContent = title;

            this.modalInstance = new bootstrap.Modal(modalEl, {
                backdrop: "static",
                keyboard: false
            });
            this.modalInstance.show();

            // Re-bind jquery validation for dynamic content (if available)
            if (window.jQuery && window.jQuery.validator) {
                const form = modalEl.querySelector("form");
                if (form) {
                    window.jQuery.validator.unobtrusive.parse(form);
                }
            }
        } catch (error) {
            console.error("Failed to load modal form:", error);
        }
    }

    close() {
        if (this.modalInstance) {
            this.modalInstance.hide();
            // Clear content after animation
            setTimeout(() => {
                const container = document.getElementById(this.containerId);
                if (container) container.innerHTML = "";
            }, 300);
        }
    }
}
