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

// Floating Toast Notification System using SweetAlert2
function bmsAlert(message, type = "success", duration = 4000) {
    if (window.Swal) {
        const Toast = Swal.mixin({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: duration,
            timerProgressBar: true,
            customClass: {
                popup: 'glass-toast'
            },
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer)
                toast.addEventListener('mouseleave', Swal.resumeTimer)
            }
        });

        Toast.fire({
            icon: type === 'danger' ? 'error' : type,
            title: message
        });
    } else {
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

        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alertEl);
            bsAlert.close();
        }, duration);
    }
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

// Compatibility aliases for older views referencing BusinessManagement names
window.BusinessManagementAjax = bmsAjax;
window.BusinessManagementAlert = bmsAlert;
window.BusinessManagementModalManager = BmsModalManager;

// Generic Excel export function (exports only visible rows and columns using server-side styled EPPlus)
function exportTableToExcel(tableId, filename) {
    const table = document.getElementById(tableId);
    if (!table) return;

    // Clone the table to manipulate it without affecting the live DOM
    const clonedTable = table.cloneNode(true);
    
    // Find all rows in the cloned table
    const tbody = clonedTable.getElementsByTagName('tbody')[0];
    const thead = clonedTable.getElementsByTagName('thead')[0];
    
    if (!tbody) return;

    // Remove hidden rows and collapsible detail rows from clone
    const rows = Array.from(tbody.rows);
    const originalTbody = table.getElementsByTagName('tbody')[0];
    
    for (let i = rows.length - 1; i >= 0; i--) {
        const originalRow = originalTbody.rows[i];
        const row = rows[i];
        
        // If the original row is hidden or is a collapsible details row, remove it from clone
        if (originalRow && (originalRow.style.display === 'none' || row.classList.contains('collapse'))) {
            row.parentNode.removeChild(row);
        }
    }

    // Identify columns to exclude (e.g. actions, details)
    const headerRow = thead ? thead.rows[0] : null;
    const indicesToRemove = [];
    
    if (headerRow) {
        const ths = Array.from(headerRow.cells);
        for (let j = ths.length - 1; j >= 0; j--) {
            const text = ths[j].innerText.trim();
            if (text === "الإجراءات" || text === "إلغاء الفاتورة" || text === "خيارات" || text === "التفاصيل" || text === "عرض البنود" || text === "العمليات") {
                indicesToRemove.push(j);
            }
        }
    }

    // Now remove these columns from all rows in the cloned table (thead, tbody, tfoot)
    const allRows = Array.from(clonedTable.querySelectorAll('tr'));
    allRows.forEach(row => {
        // Sort indices in descending order to avoid shift issues when deleting
        indicesToRemove.sort((a, b) => b - a);
        indicesToRemove.forEach(index => {
            if (row.cells[index]) {
                row.deleteCell(index);
            }
        });
    });

    // Parse headers (from first row of thead)
    const headerCells = thead ? thead.rows[0].cells : [];
    const headers = Array.from(headerCells).map(cell => cell.innerText.trim());

    const rowsData = [];

    // Parse summary row if present (it's the second row of thead)
    const summaryRowEl = thead ? thead.querySelector('tr#summaryRow') : null;
    if (summaryRowEl) {
        const rowData = [];
        const cells = summaryRowEl.cells;
        for (let i = 0; i < cells.length; i++) {
            const cell = cells[i];
            const colspan = parseInt(cell.getAttribute('colspan') || '1', 10);
            const text = cell.innerText.trim();
            rowData.push(text);
            for (let c = 1; c < colspan; c++) {
                rowData.push('');
            }
        }
        rowsData.push(rowData);
    }

    // Parse tbody rows
    const bodyRows = Array.from(tbody.rows);
    bodyRows.forEach(row => {
        const rowData = [];
        const cells = row.cells;
        for (let i = 0; i < cells.length; i++) {
            const cell = cells[i];
            const colspan = parseInt(cell.getAttribute('colspan') || '1', 10);
            const text = cell.innerText.trim();
            rowData.push(text);
            for (let c = 1; c < colspan; c++) {
                rowData.push('');
            }
        }
        rowsData.push(rowData);
    });

    // Configure alignments and text formatting specifically for bookingsTable columns
    // Right-aligned: اسم المسافر (3), التبعية (5)
    // Text format: رقم الملف (1), جواز السفر (4), باركود (7)
    let rightAlignCols = [];
    let textFormatCols = [];
    if (tableId === 'bookingsTable') {
        rightAlignCols = [3, 5];
        textFormatCols = [1, 4, 7];
    }

    // Get antiforgery token from page layout
    const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
    const token = tokenEl ? tokenEl.value : "";

    // Create a dynamic form to submit a POST request for file download
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '/Bookings/ExportCustomExcel';
    form.style.display = 'none';

    const addHiddenInput = (name, value) => {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = name;
        input.value = value;
        form.appendChild(input);
    };

    addHiddenInput('sheetName', filename);
    addHiddenInput('headersJson', JSON.stringify(headers));
    addHiddenInput('rowsJson', JSON.stringify(rowsData));
    addHiddenInput('rightAlignColsJson', JSON.stringify(rightAlignCols));
    addHiddenInput('textFormatColsJson', JSON.stringify(textFormatCols));
    if (token) {
        addHiddenInput('__RequestVerificationToken', token);
    }

    document.body.appendChild(form);
    form.submit();
    document.body.removeChild(form);
}
