document.addEventListener("DOMContentLoaded", () => {
    const input = document.getElementById("searchAjax");
    const tableBody = document.getElementById("ajaxResults");

    input.addEventListener("input", async () => {
        const text = input.value;

        const url = `/api/tasks/ajax-search?text=${encodeURIComponent(text)}`;

    try {
        const response = await fetch(url);
        if (!response.ok) throw new Error("Error en la API");

        const data = await response.json();

        // Limpiar tabla
        tableBody.innerHTML = "";

        // Insertar filas
        data.forEach(item => {
            const row = `
                    <tr>
                        <td>${item.title}</td>
                        <td>${item.categoryName}</td>
                        <td>${item.step}</td>
                        <td>${item.isCompleted ? "Sí" : "No"}</td>
                        <td>
                            <a href="/Tasks/Details/${item.id}" class="btn btn-sm btn-info">Ver</a>
                        </td>
                    </tr>
                `;
            tableBody.innerHTML += row;
        });

    } catch (err) {
        console.error("Error:", err);
    }
});
});