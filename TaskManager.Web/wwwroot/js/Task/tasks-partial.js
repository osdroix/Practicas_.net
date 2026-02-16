document.addEventListener("DOMContentLoaded", () => {
    const btnBuscar = document.getElementById("btnBuscarAjax");
    const contenedor = document.getElementById("taskTableContainer");
    const formulario = document.getElementById("filterForm");

  
    btnBuscar.addEventListener("click", async () => {
        // 1. Convertimos el formulario en QueryString
        const formData = new FormData(formulario);
        const query = new URLSearchParams();

        formData.forEach((value, key) => {
            if (value !== null && value !== "") {
                query.append(key, value);
            }
        });

        // 2. Construimos la URL
        const url = '/Tasks/LoadTablePartial?' + query.toString();

        try {
            // --- TODO ESTO DEBÍA IR DENTRO DEL EVENTO ---
            const response = await fetch(url);

            if (!response.ok) {
                contenedor.innerHTML = "<p>Error al cargar resultados.</p>";
                return;
            }

            const html = await response.text();

            // 3. Reemplazamos la tabla
            contenedor.innerHTML = html;

        } catch (error) {
            console.error("Error en la petición:", error);
            contenedor.innerHTML = "<p>Hubo un fallo en la conexión.</p>";
        }
    }); 
});