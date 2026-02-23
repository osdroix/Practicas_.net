document.addEventListener("DOMContentLoaded", () => {

    const modal = new bootstrap.Modal(document.getElementById("taskModal"));
    const modalContent = document.getElementById("taskModalContent");
    let isSubmitting = false;

    // CREAR
    const createButton = document.getElementById("btnCrearTask");
    if (createButton) {
        createButton.addEventListener("click", async () => {
            modalContent.innerHTML = spinnerHtml();

            const response = await fetch("/Tasks/CreatePartial");
            const html = await response.text();

            modalContent.innerHTML = html;
            initializeTaskForm();
            modal.show();
        });
    }

    // EDITAR
    document.addEventListener("click", async (e) => {
        if (e.target.matches(".btnEdit")) {
            const id = e.target.dataset.id;

            modalContent.innerHTML = spinnerHtml();

            const response = await fetch("/Tasks/EditPartial/" + id);
            const html = await response.text();

            modalContent.innerHTML = html;
            initializeTaskForm();
            modal.show();
        }
    });

    // GUARDAR
    document.addEventListener("click", async (e) => {
        if (e.target.id === "btnSaveTask") {
            e.preventDefault();

            if (isSubmitting) return;

            const form = document.getElementById("taskForm");
            if (!form) return;

            clearValidationMessages();

            const validationResult = validateTaskForm();
            if (!validationResult.isValid) {
                showFormSummary("Por favor corrige los errores marcados en el formulario.");
                return;
            }

            isSubmitting = true;
            const saveButton = document.getElementById("btnSaveTask");
            const originalText = saveButton.textContent;
            saveButton.textContent = "Guardando...";
            saveButton.disabled = true;

            const id = validationResult.values.id;
            const url = id && id !== "0"
                ? "/Tasks/Edit/" + id
                : "/Tasks/Create";

            const payload = new URLSearchParams();
            payload.append("Id", validationResult.values.id);
            payload.append("Title", validationResult.values.title);
            payload.append("CategoryId", validationResult.values.categoryId);
            payload.append("Step", validationResult.values.step.toString());
            payload.append("IsCompleted", validationResult.values.isCompleted ? "true" : "false");

            try {
            const response = await fetch(url, {
                method: "POST",
                headers: {
                    "X-Requested-With": "XMLHttpRequest",
                    "Accept": "application/json"
                },
                body: payload
            });

            const contentType = response.headers.get("content-type") || "";

            if (!response.ok) {
                if (contentType.includes("application/json")) {
                    const data = await response.json();
                    if (data?.errors) {
                        applyServerErrors(data.errors);
                    }
                    showFormSummary(data?.message || "Ocurrió un error al guardar la tarea.");
                } else {
                    const text = await response.text();
                    showFormSummary(text || "Ocurrió un error al guardar la tarea.");
                }
                return;
            }

            if (contentType.includes("application/json")) {
                await response.json();
            }

            modal.hide();
            await refreshTable();
            } catch (error) {
                console.error("Network error while saving task:", error);
                showFormSummary("No se pudo conectar con el servidor. Verifica tu conexión e inténtalo otra vez.");
            } finally {
                isSubmitting = false;
                saveButton.textContent = originalText;
                saveButton.disabled = false;
            }
        }
    });

});

function initializeTaskForm() {
    // Si el select ya viene precargado desde el servidor, no se vuelve a pedir por fetch
    const categorySelect = document.getElementById("CategoryId");
    const selectedCategory = categorySelect ? categorySelect.dataset.selectedCategory : "";
    const isPreloaded = categorySelect?.dataset.preloaded === "true";
    if (!isPreloaded) {
        loadCategories(categorySelect, selectedCategory);
    } else if (selectedCategory) {
        categorySelect.value = selectedCategory.toString();
    }

    const isCompletedSwitch = document.getElementById("IsCompletedSwitch");
    const stepGroup = document.getElementById("stepGroup");

    if (isCompletedSwitch && stepGroup) {
        const toggleStepVisibility = () => {
            const isCompleted = isCompletedSwitch.checked;
            stepGroup.classList.toggle("opacity-50", isCompleted);
        };

        isCompletedSwitch.addEventListener("change", toggleStepVisibility);
        toggleStepVisibility();
    }

    const titleInput = document.getElementById("Title");
    if (titleInput) {
        titleInput.addEventListener("input", () => validateField("Title"));
    }
    if (categorySelect) {
        categorySelect.addEventListener("change", () => validateField("CategoryId"));
    }
    const stepInput = document.getElementById("Step");
    if (stepInput) {
        stepInput.addEventListener("input", () => validateField("Step"));
    }
}

// Carga categorías desde endpoints JSON con fallback
async function loadCategories(selectElement, selectedCategoryId) {
    if (!selectElement) return;

    selectElement.innerHTML = '<option value="">Cargando categorías...</option>';
    selectElement.disabled = true;

    try {
        // Endpoint principal
        let response = await fetch("/api/categories");

        if (!response.ok || !(response.headers.get("content-type") || "").includes("application/json")) {
            // Fallback cuando /api/categories no está disponible desde el navegador
            response = await fetch("/Categories/GetCategoriesList");
        }

        if (!response.ok || !(response.headers.get("content-type") || "").includes("application/json")) {
            throw new Error("Respuesta no válida de la API de categorías");
        }

        const categories = await response.json();

        selectElement.innerHTML = '<option value="">Seleccione una categoría</option>';

        // Soporta distintas variantes de nombres de propiedades
        categories.forEach(cat => {
            const option = document.createElement("option");
            const rawId = cat.id ?? cat.Id ?? cat.categoryId ?? cat.CategoryId ?? "";
            const rawName = cat.name ?? cat.Name ?? cat.categoryName ?? cat.CategoryName ?? "Sin nombre";
            option.value = rawId.toString();
            option.textContent = rawName;
            selectElement.appendChild(option);
        });

        if (selectedCategoryId) {
            selectElement.value = selectedCategoryId.toString();
        }

        selectElement.disabled = false;
    } catch (error) {
        console.error("Error cargando categorías:", error);
        selectElement.innerHTML = '<option value="">No se pudieron cargar las categorías</option>';
        selectElement.disabled = true;
        showFormSummary("No se pudieron cargar las categorías. Intenta recargar la página.");
    }
}

function validateTaskForm() {
    const title = document.getElementById("Title")?.value.trim() ?? "";
    const categoryId = document.getElementById("CategoryId")?.value ?? "";
    const stepValue = document.getElementById("Step")?.value ?? "";
    const isCompleted = document.getElementById("IsCompletedSwitch")?.checked ?? false;
    const id = document.querySelector("#taskForm input[name='Id']")?.value ?? "0";

    let isValid = true;

    if (!title) {
        setFieldError("Title", "El título es obligatorio.");
        isValid = false;
    }

    if (!categoryId || categoryId === "0") {
        setFieldError("CategoryId", "Debe seleccionar una categoría.");
        isValid = false;
    }

    const step = Number(stepValue);
    if (!stepValue || Number.isNaN(step) || step < 1 || step > 5) {
        setFieldError("Step", "El step debe estar entre 1 y 5.");
        isValid = false;
    }

    return {
        isValid,
        values: {
            id,
            title,
            categoryId,
            step: step || 1,
            isCompleted
        }
    };
}

function validateField(fieldName) {
    clearFieldError(fieldName);

    if (fieldName === "Title") {
        const title = document.getElementById("Title")?.value.trim() ?? "";
        if (!title) {
            setFieldError("Title", "El título es obligatorio.");
        }
    }

    if (fieldName === "CategoryId") {
        const categoryId = document.getElementById("CategoryId")?.value ?? "";
        if (!categoryId || categoryId === "0") {
            setFieldError("CategoryId", "Debe seleccionar una categoría.");
        }
    }

    if (fieldName === "Step") {
        const stepValue = document.getElementById("Step")?.value ?? "";
        const step = Number(stepValue);
        if (!stepValue || Number.isNaN(step) || step < 1 || step > 5) {
            setFieldError("Step", "El step debe estar entre 1 y 5.");
        }
    }
}

function clearValidationMessages() {
    document.querySelectorAll("[data-valmsg-for]").forEach(span => {
        span.textContent = "";
    });
    const summary = document.getElementById("formErrorSummary");
    if (summary) {
        summary.textContent = "";
        summary.classList.add("d-none");
    }
}

function setFieldError(fieldName, message) {
    const span = document.querySelector(`[data-valmsg-for='${fieldName}']`);
    if (span) {
        span.textContent = message;
    }
}

function clearFieldError(fieldName) {
    const span = document.querySelector(`[data-valmsg-for='${fieldName}']`);
    if (span) {
        span.textContent = "";
    }
}

function showFormSummary(message) {
    const summary = document.getElementById("formErrorSummary");
    if (summary) {
        summary.textContent = message;
        summary.classList.remove("d-none");
    }
}

function applyServerErrors(errors) {
    if (!errors || typeof errors !== "object") return;

    Object.keys(errors).forEach(key => {
        const messages = errors[key];
        if (Array.isArray(messages) && messages.length > 0) {
            setFieldError(key, messages[0]);
        }
    });
}

function spinnerHtml() {
    return `
        <div class="modal-body text-center">
            <div class="spinner-border text-primary"></div>
            <p>Cargando...</p>
        </div>`;
}

async function refreshTable() {
    const response = await fetch("/Tasks/LoadTablePartial");
    const html = await response.text();
    document.getElementById("taskTableContainer").innerHTML = html;
}
