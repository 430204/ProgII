// Pantalla de operaciones: publicar, concretar, facturar y cambiar estado.
// Cada accion ejecuta un procedimiento almacenado y muestra que hizo la base.
// API: GET /Operacion/catalogos, POST /Operacion/{publicar|concretar|facturar|cambiar-estado}
(function () {
    "use strict";

    const $ = function (id) { return document.getElementById(id); };

    let catalogos = null;

    // ---------------------------------------------------------------- formato
    function numero(valor, decimales) {
        if (valor === null || valor === undefined) return "-";
        return Number(valor).toLocaleString("es-AR", {
            minimumFractionDigits: decimales === undefined ? 2 : decimales,
            maximumFractionDigits: decimales === undefined ? 2 : decimales
        });
    }

    function hoy() {
        const d = new Date();
        const mes = String(d.getMonth() + 1).padStart(2, "0");
        const dia = String(d.getDate()).padStart(2, "0");
        return d.getFullYear() + "-" + mes + "-" + dia;
    }

    function claseEstado(estado) {
        const e = (estado || "").toLowerCase();
        if (e === "disponible") return "crud-badge crud-badge-disponible";
        if (e === "vendido") return "crud-badge crud-badge-vendido";
        if (e === "alquilado") return "crud-badge crud-badge-alquilado";
        if (e === "reservado") return "crud-badge crud-badge-reservado";
        return "crud-badge";
    }

    async function textoDeError(respuesta) {
        try {
            const datos = await respuesta.json();
            if (datos.mensaje) return datos.mensaje;
            if (datos.errors) return Object.values(datos.errors).flat().join(" ");
            if (datos.title) return datos.title;
        } catch (e) { /* sin cuerpo JSON */ }
        return "No se pudo completar la operación (" + respuesta.status + ").";
    }

    function llenarSelect(id, opciones, textoVacio) {
        const select = $(id);
        select.innerHTML = "";

        if (textoVacio !== undefined) {
            const vacio = document.createElement("option");
            vacio.value = "";
            vacio.textContent = textoVacio;
            select.appendChild(vacio);
        }

        opciones.forEach(function (o) {
            const opcion = document.createElement("option");
            opcion.value = o.valor;
            opcion.textContent = o.texto;
            select.appendChild(opcion);
        });
    }

    function desdeOpciones(lista) {
        return lista.map(function (o) {
            return { valor: o.id, texto: o.texto };
        });
    }

    // ---------------------------------------------------------------- resultado
    function mostrarError(id, texto) {
        const caja = $(id);
        caja.className = "op-resultado op-error";
        caja.innerHTML = "";
        const p = document.createElement("p");
        p.className = "op-resultado-titulo";
        p.textContent = texto;
        caja.appendChild(p);
        caja.hidden = false;
    }

    function mostrarResultado(id, datos) {
        const caja = $(id);
        caja.className = "op-resultado op-exito";
        caja.innerHTML = "";

        const titulo = document.createElement("p");
        titulo.className = "op-resultado-titulo";
        titulo.textContent = datos.mensaje;
        caja.appendChild(titulo);

        if (datos.estadoAnterior && datos.estadoNuevo &&
            datos.estadoAnterior !== datos.estadoNuevo) {
            const transicion = document.createElement("div");
            transicion.className = "op-transicion";

            const antes = document.createElement("span");
            antes.className = claseEstado(datos.estadoAnterior);
            antes.textContent = datos.estadoAnterior;
            transicion.appendChild(antes);

            const flecha = document.createElement("span");
            flecha.className = "op-flecha";
            flecha.textContent = "→";
            transicion.appendChild(flecha);

            const despues = document.createElement("span");
            despues.className = claseEstado(datos.estadoNuevo);
            despues.textContent = datos.estadoNuevo;
            transicion.appendChild(despues);

            caja.appendChild(transicion);
        }

        if (datos.efectosEnLaBase && datos.efectosEnLaBase.length > 0) {
            const lista = document.createElement("ul");
            lista.className = "op-efectos";
            datos.efectosEnLaBase.forEach(function (texto) {
                const li = document.createElement("li");
                li.textContent = texto;
                lista.appendChild(li);
            });
            caja.appendChild(lista);
        }

        caja.hidden = false;
    }

    // ---------------------------------------------------------------- envio
    async function enviar(url, cuerpo, idResultado, formulario) {
        const boton = formulario.querySelector("button[type=submit]");
        boton.disabled = true;

        try {
            const respuesta = await fetch(url, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(cuerpo)
            });

            if (!respuesta.ok) {
                mostrarError(idResultado, await textoDeError(respuesta));
                return false;
            }

            mostrarResultado(idResultado, await respuesta.json());
            await cargarCatalogos();
            return true;

        } catch (error) {
            mostrarError(idResultado, "No se pudo conectar con el servidor.");
            console.error(error);
            return false;
        } finally {
            boton.disabled = false;
        }
    }

    // ---------------------------------------------------------------- catalogos
    async function cargarCatalogos() {
        const respuesta = await fetch("/Operacion/catalogos");
        if (!respuesta.ok) throw new Error("No se pudieron cargar los catálogos.");
        catalogos = await respuesta.json();

        // 1. Publicar
        llenarSelect("pub-inmueble", desdeOpciones(catalogos.publicables),
                     catalogos.publicables.length === 0
                         ? "No hay propiedades disponibles sin publicar"
                         : "Elegí una propiedad");
        llenarSelect("pub-operacion-tipo", desdeOpciones(catalogos.operaciones));
        llenarSelect("pub-moneda-nueva", catalogos.monedas.map(function (m) {
            return { valor: m, texto: m };
        }));

        // 2. Concretar
        llenarSelect("con-publicacion", catalogos.publicaciones.map(function (p) {
            return {
                valor: p.idPublicacion,
                texto: p.codigo + " - " + p.titulo + " (" + p.operacion + ", " +
                       p.moneda + " " + numero(p.precio) + ")"
            };
        }), catalogos.publicaciones.length === 0
                ? "No hay publicaciones activas"
                : "Elegí una publicación");
        llenarSelect("con-cliente", desdeOpciones(catalogos.clientes), "Elegí el cliente");
        llenarSelect("con-agente", desdeOpciones(catalogos.agentes), "Elegí el agente");

        // 3. Facturar
        llenarSelect("fac-contrato", catalogos.contratos.map(function (c) {
            return {
                valor: c.idContrato,
                texto: "#" + c.idContrato + " - " + c.codigo + " - " + c.cliente +
                       (c.cantidadFacturas > 0
                           ? " (" + c.cantidadFacturas + " factura/s)"
                           : "")
            };
        }), catalogos.contratos.length === 0
                ? "Todavía no hay contratos"
                : "Elegí un contrato");

        // 4. Cambiar estado
        llenarSelect("est-inmueble", desdeOpciones(catalogos.inmuebles), "Elegí un inmueble");
        llenarSelect("est-estado", catalogos.estados.map(function (e) {
            return { valor: e, texto: e };
        }), "Elegí el estado");
    }

    // ------------------------------------------------- publicacion seleccionada
    function publicacionElegida() {
        const id = Number($("con-publicacion").value);
        return catalogos.publicaciones.find(function (p) {
            return p.idPublicacion === id;
        }) || null;
    }

    $("con-publicacion").addEventListener("change", function () {
        const p = publicacionElegida();

        if (!p) {
            $("con-detalle").textContent = "";
            return;
        }

        // El monto arranca en el precio publicado y la comisión según el tipo
        $("con-monto").value = p.precio;
        $("con-comision").value = p.operacion === "Alquiler" ? "8" : "3";
        $("con-detalle").textContent =
            p.operacion + " en " + p.ciudad + " · precio publicado " +
            p.moneda + " " + numero(p.precio);

        // Un alquiler necesita fecha de fin; una venta no
        if (p.operacion === "Alquiler") {
            const fin = new Date();
            fin.setFullYear(fin.getFullYear() + 2);
            $("con-fin").value = fin.toISOString().substring(0, 10);
        } else {
            $("con-fin").value = "";
        }
    });

    $("fac-contrato").addEventListener("change", function () {
        const id = Number($("fac-contrato").value);
        const c = (catalogos.contratos || []).find(function (x) {
            return x.idContrato === id;
        });

        if (!c) {
            $("fac-detalle").textContent = "";
            return;
        }

        $("fac-precio").value = c.comisionSugerida;
        $("fac-concepto").value = "Comisión - " + c.codigo;
        $("fac-numero").value = "FC-A-" + new Date().getFullYear() + "-" +
                                String(c.idContrato).padStart(4, "0");
        $("fac-detalle").textContent =
            "Monto del contrato " + c.moneda + " " + numero(c.montoAcordado) +
            " · comisión " + numero(c.comisionPorcentaje) + " % = " +
            c.moneda + " " + numero(c.comisionSugerida);
    });

    // ---------------------------------------------------------------- formularios
    $("form-publicar").addEventListener("submit", async function (evento) {
        evento.preventDefault();
        await enviar("/Operacion/publicar", {
            idInmueble: Number($("pub-inmueble").value),
            idTipoOperacion: Number($("pub-operacion-tipo").value),
            precio: Number($("pub-precio").value),
            moneda: $("pub-moneda-nueva").value
        }, "resultado-publicar", this);
        this.reset();
    });

    $("form-concretar").addEventListener("submit", async function (evento) {
        evento.preventDefault();
        const fin = $("con-fin").value;
        await enviar("/Operacion/concretar", {
            idPublicacion: Number($("con-publicacion").value),
            idCliente: Number($("con-cliente").value),
            idAgente: Number($("con-agente").value),
            fechaInicio: $("con-inicio").value || null,
            fechaFin: fin === "" ? null : fin,
            monto: Number($("con-monto").value),
            comision: Number($("con-comision").value)
        }, "resultado-concretar", this);
        this.reset();
        $("con-inicio").value = hoy();
        $("con-detalle").textContent = "";
    });

    $("form-facturar").addEventListener("submit", async function (evento) {
        evento.preventDefault();
        const vencimiento = $("fac-vencimiento").value;
        await enviar("/Operacion/facturar", {
            numeroFactura: $("fac-numero").value,
            idContrato: Number($("fac-contrato").value),
            fechaVencimiento: vencimiento === "" ? null : vencimiento,
            concepto: $("fac-concepto").value,
            cantidad: Number($("fac-cantidad").value),
            precioUnitario: Number($("fac-precio").value)
        }, "resultado-facturar", this);
        this.reset();
        $("fac-cantidad").value = 1;
        $("fac-detalle").textContent = "";
    });

    $("form-estado").addEventListener("submit", async function (evento) {
        evento.preventDefault();
        await enviar("/Operacion/cambiar-estado", {
            idInmueble: Number($("est-inmueble").value),
            estado: $("est-estado").value
        }, "resultado-estado", this);
        this.reset();
    });

    // ---------------------------------------------------------------- inicio
    (async function iniciar() {
        $("con-inicio").value = hoy();

        try {
            await cargarCatalogos();
        } catch (error) {
            mostrarError("resultado-concretar", "No se pudo conectar con el servidor.");
            console.error(error);
        }
    })();
})();
