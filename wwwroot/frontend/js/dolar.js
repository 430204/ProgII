
(() => {
    const API_URL = "https://dolarapi.com/v1/dolares/blue";

    const formatoMoneda = new Intl.NumberFormat("es-AR", {
        style: "currency",
        currency: "ARS",
        maximumFractionDigits: 0
    });

    async function cargarCotizacion() {

        const compra = document.getElementById("dolar-compra");
        const venta = document.getElementById("dolar-venta");
        const estado = document.getElementById("dolar-estado");
        const fecha = document.getElementById("dolar-fecha");

        // Comprobar que existan los elementos HTML
        if (!compra || !venta || !estado || !fecha) {
            console.error("No se encontraron los elementos HTML del dólar.");
            return;
        }

        try {
            estado.textContent = "Actualizando cotización...";

            const response = await fetch(API_URL);

            if (!response.ok) {
                throw new Error(`Error HTTP: ${response.status}`);
            }

            const data = await response.json();

            // Validar los datos recibidos
            if (
                typeof data.compra !== "number" ||
                typeof data.venta !== "number" ||
                !data.fechaActualizacion
            ) {
                throw new Error("La respuesta de la API no tiene el formato esperado.");
            }

            // Actualizar compra y venta
            compra.textContent = formatoMoneda.format(data.compra);
            venta.textContent = formatoMoneda.format(data.venta);

            // Formatear fecha
            const fechaActualizacion = new Date(data.fechaActualizacion);

            fecha.textContent = "Actualizado: " +
                fechaActualizacion.toLocaleString("es-AR");

            estado.textContent = "Cotización actualizada correctamente";

            console.log("Cotización recibida:", data);

        } catch (error) {

            console.error("Error al cargar el dólar:", error);

            estado.textContent = "Error al consultar la cotización";
        }
    }

    // Ejecutar cuando el documento esté listo
    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", cargarCotizacion);
    } else {
        cargarCotizacion();
    }

    // Actualizar cada 5 minutos
    setInterval(cargarCotizacion, 5 * 60 * 1000);

})();
