
const cantidad = document.getElementById("cantidad");
const precioUnitario = document.getElementById("precio-unitario");
const subtotal = document.getElementById("subtotal");

const formularioDetalle = document.getElementById("formulario-detalle");
const mensajeDetalle = document.getElementById("mensaje-detalle");
const botonGuardar = document.getElementById("guardar-detalle");
const botonCancelar = document.getElementById("cancelar-edicion");

const idDetalle = document.getElementById("id-detalle");
const selectorFactura = document.getElementById("id-factura");
const concepto = document.getElementById("concepto");

// CALCULAR SUBTOTAL
function calcularSubtotal() {
    const total = Number(cantidad.value) * Number(precioUnitario.value);

    subtotal.textContent = total.toLocaleString("es-AR", {
        style: "currency",
        currency: "ARS"
    });
}

cantidad.addEventListener("input", calcularSubtotal);
precioUnitario.addEventListener("input", calcularSubtotal);

let facturasCargadas = [];
// CARGAR FACTURAS
async function cargarFacturas() {
    try {
        const respuesta = await fetch("/api/Factura");

        if (!respuesta.ok) {
            throw new Error("Error al obtener las facturas");
        }

        const facturas = await respuesta.json();
        facturasCargadas = facturas;

        selectorFactura.innerHTML =
            '<option value="">Seleccionar factura</option>';

        facturas.forEach(factura => {
            const opcion = document.createElement("option");

            opcion.value = factura.idFactura;
            opcion.textContent =
                factura.numeroFactura + " - " + factura.estado;

            selectorFactura.appendChild(opcion);
        });

    } catch (error) {
        console.error(error);
        selectorFactura.innerHTML =
            '<option value="">Error al cargar facturas</option>';

        mensajeDetalle.textContent = "No se pudieron cargar las facturas.";
    }
}

// CARGAR DETALLES
async function cargarDetalles() {
    const tabla = document.getElementById("tabla-detalles");

    try {
        const respuesta = await fetch("/api/DetalleFactura");

        if (!respuesta.ok) {
            throw new Error("Error al obtener los detalles");
        }

        const detalles = await respuesta.json();

        tabla.innerHTML = "";

        detalles.forEach(detalle => {
            const fila = document.createElement("tr");

            const factura = facturasCargadas.find(
                f => f.idFactura === detalle.idFactura
            );

            const datos = [
                detalle.idDetalleFactura,
                factura ? factura.numeroFactura : detalle.idFactura,
                detalle.concepto,
                detalle.cantidad,
                detalle.precioUnitario.toLocaleString("es-AR", {
                    style: "currency",
                    currency: "ARS"
                }),
                detalle.subtotal.toLocaleString("es-AR", {
                    style: "currency",
                    currency: "ARS"
                })
            ];

            datos.forEach(dato => {
                const celda = document.createElement("td");
                celda.textContent = dato;
                fila.appendChild(celda);
            });

            const celdaAcciones = document.createElement("td");

            // BOTON EDITAR
            const botonEditar = document.createElement("button");
            botonEditar.textContent = "Editar";
            botonEditar.type = "button";
            botonEditar.classList.add("btn-editar");

            botonEditar.addEventListener("click", function () {
                idDetalle.value = detalle.idDetalleFactura;
                selectorFactura.value = detalle.idFactura;
                concepto.value = detalle.concepto;
                cantidad.value = detalle.cantidad;
                precioUnitario.value = detalle.precioUnitario;

                botonGuardar.textContent = "Guardar cambios";
                mensajeDetalle.textContent = "";

                calcularSubtotal();

                formularioDetalle.scrollIntoView({
                    behavior: "smooth",
                    block: "start"
                });
            });

            // BOTON ELIMINAR
            const botonEliminar = document.createElement("button");
            botonEliminar.textContent = "Eliminar";
            botonEliminar.type = "button";
            botonEliminar.classList.add("btn-eliminar");

            botonEliminar.addEventListener("click", async function () {
                const confirmar = confirm(
                    "¿Seguro que querés eliminar este detalle?"
                );

                if (!confirmar) {
                    return;
                }

                botonEliminar.disabled = true;

                try {
                    const respuesta = await fetch(
                        `/api/DetalleFactura/${detalle.idDetalleFactura}`,
                        {
                            method: "DELETE"
                        }
                    );

                    if (!respuesta.ok) {
                        throw new Error(await respuesta.text());
                    }

                    if (idDetalle.value === String(detalle.idDetalleFactura)) {
                        limpiarFormulario();
                    }

                    mensajeDetalle.textContent =
                        "Detalle eliminado correctamente.";

                    await cargarDetalles();

                } catch (error) {
                    console.error(error);
                    mensajeDetalle.textContent =
                        "No se pudo eliminar el detalle.";

                    botonEliminar.disabled = false;
                }
            });

            celdaAcciones.appendChild(botonEditar);
            celdaAcciones.appendChild(botonEliminar);

            fila.appendChild(celdaAcciones);
            tabla.appendChild(fila);
        });

    } catch (error) {
        console.error(error);
        tabla.innerHTML = "";

        const fila = document.createElement("tr");
        const celda = document.createElement("td");

        celda.colSpan = 7;
        celda.textContent = "No se pudieron cargar los detalles.";

        fila.appendChild(celda);
        tabla.appendChild(fila);
    }
}

// LIMPIAR FORMULARIO
function limpiarFormulario() {
    formularioDetalle.reset();

    idDetalle.value = "";
    cantidad.value = 1;

    botonGuardar.textContent = "Guardar detalle";

    calcularSubtotal();
}

// CANCELAR EDICION
botonCancelar.addEventListener("click", function () {
    limpiarFormulario();
    mensajeDetalle.textContent = "";
});

// GUARDAR O MODIFICAR DETALLE
formularioDetalle.addEventListener("submit", async function (event) {
    event.preventDefault();

    const detalle = {
        idFactura: Number(selectorFactura.value),
        concepto: concepto.value.trim(),
        cantidad: Number(cantidad.value),
        precioUnitario: Number(precioUnitario.value)
    };

    if (!detalle.idFactura ||
        !detalle.concepto ||
        !Number.isInteger(detalle.cantidad) ||
        detalle.cantidad <= 0 ||
        !Number.isFinite(detalle.precioUnitario) ||
        detalle.precioUnitario < 0) {

        mensajeDetalle.textContent =
            "Completá los campos correctamente.";
        return;
    }

    const editando = idDetalle.value !== "";

    const url = editando
        ? `/api/DetalleFactura/${idDetalle.value}`
        : "/api/DetalleFactura";

    const metodo = editando ? "PUT" : "POST";

    botonGuardar.disabled = true;

    try {
        const respuesta = await fetch(url, {
            method: metodo,
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(detalle)
        });

        if (!respuesta.ok) {
            throw new Error(await respuesta.text());
        }

        limpiarFormulario();

        mensajeDetalle.textContent = editando
            ? "Detalle modificado correctamente."
            : "Detalle guardado correctamente.";

        await cargarDetalles();

    } catch (error) {
        console.error(error);
        mensajeDetalle.textContent =
            "No se pudo guardar el detalle.";
    } finally {
        botonGuardar.disabled = false;
    }
});

// CARGA INICIAL
calcularSubtotal();

async function iniciarPagina() {
    await cargarFacturas();
    await cargarDetalles();
}

iniciarPagina();
