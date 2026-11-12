// ABM de propiedades, integrado en Indice.html.
// Ciclo de vida: el alta crea la propiedad Disponible y la edicion es la que la
// pasa a Vendido, Alquilado o Reservado (y da de baja sus publicaciones activas).
// API: GET/POST /Inmueble/propiedades, GET/PUT/DELETE /Inmueble/propiedades/{id}, GET /Inmueble/catalogos
(function () {
    "use strict";

    const API = "/Inmueble";
    const $ = function (id) { return document.getElementById(id); };

    const mensaje = $("mensaje-inactivas");
    const mensajeDialogo = $("mensaje-dialogo");
    const cuerpo = $("cuerpo-inactivas");
    const cantidad = $("cantidad-inactivas");
    const formFiltros = $("formulario-filtros-inactivas");
    const form = $("formulario-inmueble");
    const dialogo = $("dialogo-inmueble");
    const dialogoEliminar = $("dialogo-eliminar");
    const selectEstado = $("inm-estado");
    const ayudaEstado = $("ayuda-estado");

    let idEditando = null;        // null = alta, número = edición
    let estadoAnterior = null;    // estado que tenía la propiedad antes de editarla
    let pendienteEliminar = null; // { id, codigo }
    let catalogos = null;         // respuesta de /Inmueble/catalogos

    // ---------- Utilidades ----------
    function mostrar(elemento, texto, tipo) {
        elemento.textContent = texto;
        if (tipo) elemento.className = "crud-mensaje " + tipo;
        elemento.hidden = false;
    }

    function ocultar(elemento) {
        elemento.textContent = "";
        elemento.hidden = true;
    }

    // Lee el error de la API: {mensaje} (409/400) o ProblemDetails con errors (validación)
    async function textoDeError(respuesta) {
        try {
            const datos = await respuesta.json();
            if (datos.mensaje) return datos.mensaje;
            if (datos.errors) return Object.values(datos.errors).flat().join(" ");
            if (datos.title) return datos.title;
        } catch (e) { /* sin cuerpo JSON */ }
        return "Ocurrió un error (" + respuesta.status + ").";
    }

    function celda(fila, texto) {
        const td = document.createElement("td");
        td.textContent = texto === null || texto === undefined ? "" : texto;
        fila.appendChild(td);
        return td;
    }

    function llenarSelect(select, opciones, textoVacio) {
        select.innerHTML = "";
        if (textoVacio !== undefined) {
            const vacio = document.createElement("option");
            vacio.value = "";
            vacio.textContent = textoVacio;
            select.appendChild(vacio);
        }
        opciones.forEach(function (o) {
            const opcion = document.createElement("option");
            opcion.value = o.id;
            opcion.textContent = o.texto;
            select.appendChild(opcion);
        });
    }

    function claseEstado(estado) {
        const e = (estado || "").toLowerCase();
        if (e === "disponible") return "crud-badge crud-badge-disponible";
        if (e === "vendido") return "crud-badge crud-badge-vendido";
        if (e === "alquilado") return "crud-badge crud-badge-alquilado";
        if (e === "reservado") return "crud-badge crud-badge-reservado";
        return "crud-badge";
    }

    function textoEstado(idEstado) {
        const encontrado = catalogos.estados.find(function (e) {
            return e.id === Number(idEstado);
        });
        return encontrado ? encontrado.texto : "";
    }

    // ---------- Catálogos (desplegables) ----------
    async function cargarCatalogos() {
        const respuesta = await fetch(API + "/catalogos");
        if (!respuesta.ok) throw new Error("No se pudieron cargar los catálogos.");
        catalogos = await respuesta.json();

        llenarSelect($("inm-tipo"), catalogos.tipos);
        llenarSelect($("inm-direccion"), catalogos.direcciones);
        llenarSelect($("inm-inmobiliaria"), catalogos.inmobiliarias);
        llenarSelect(selectEstado, catalogos.estados);
        llenarSelect($("filtro-estado-inactivas"), catalogos.estados, "Todos");
    }

    // ---------- READ: listado ----------
    async function cargarListado() {
        const parametros = new URLSearchParams();
        const texto = $("filtro-texto-inactivas").value.trim();
        const estado = $("filtro-estado-inactivas").value;
        if (texto !== "") parametros.append("texto", texto);
        if (estado !== "") parametros.append("idEstado", estado);

        const respuesta = await fetch(API + "/propiedades?" + parametros.toString());
        if (!respuesta.ok) {
            mostrar(mensaje, await textoDeError(respuesta), "error");
            return;
        }
        dibujarTabla(await respuesta.json());
    }

    function dibujarTabla(lista) {
        cuerpo.innerHTML = "";
        cantidad.textContent = "Propiedades encontradas: " + lista.length;

        if (lista.length === 0) {
            const fila = document.createElement("tr");
            const td = celda(fila, "No se encontraron propiedades con esos filtros.");
            td.colSpan = 9;
            td.className = "crud-vacio";
            cuerpo.appendChild(fila);
            return;
        }

        lista.forEach(function (p) {
            const fila = document.createElement("tr");
            celda(fila, p.codigo);
            celda(fila, p.titulo);
            celda(fila, p.tipoInmueble);

            const tdEstado = document.createElement("td");
            const badge = document.createElement("span");
            badge.className = claseEstado(p.estado);
            badge.textContent = p.estado;
            tdEstado.appendChild(badge);
            fila.appendChild(tdEstado);

            celda(fila, p.calle + " " + p.numero + ", " + p.ciudad);
            celda(fila, p.superficieTotal.toLocaleString("es-AR") + " m²");
            celda(fila, p.dormitorios === null ? "-" : p.dormitorios);
            celda(fila, p.banios === null ? "-" : p.banios);

            const acciones = document.createElement("td");

            const editar = document.createElement("button");
            editar.type = "button";
            editar.className = "crud-btn crud-btn-chico crud-btn-primario";
            editar.textContent = "Editar";
            editar.addEventListener("click", function () { abrirEdicion(p.idInmueble); });
            acciones.appendChild(editar);

            const eliminar = document.createElement("button");
            eliminar.type = "button";
            eliminar.className = "crud-btn crud-btn-chico crud-btn-peligro";
            eliminar.textContent = "Eliminar";
            eliminar.addEventListener("click", function () { pedirConfirmacion(p.idInmueble, p.codigo); });
            acciones.appendChild(eliminar);

            fila.appendChild(acciones);
            cuerpo.appendChild(fila);
        });
    }

    // ---------- Formulario (alta / edición) ----------
    function numeroONull(id) {
        const valor = $(id).value;
        return valor === "" ? null : Number(valor);
    }

    function leerFormulario() {
        return {
            codigo: $("inm-codigo").value.trim(),
            titulo: $("inm-titulo").value.trim(),
            descripcion: $("inm-descripcion").value.trim() || null,
            superficieTotal: Number($("inm-sup-total").value),
            superficieCubierta: numeroONull("inm-sup-cubierta"),
            ambientes: numeroONull("inm-ambientes"),
            dormitorios: numeroONull("inm-dormitorios"),
            banios: numeroONull("inm-banios"),
            cochera: $("inm-cochera").checked,
            idDireccion: Number($("inm-direccion").value),
            idTipoInmueble: Number($("inm-tipo").value),
            idEstadoInmueble: Number(selectEstado.value),
            idInmobiliaria: Number($("inm-inmobiliaria").value)
        };
    }

    function abrirAlta() {
        idEditando = null;
        estadoAnterior = null;
        form.reset();
        ocultar(mensajeDialogo);

        // En el alta el estado no se elige: la propiedad nace Disponible.
        selectEstado.value = catalogos.estadoDisponibleId;
        selectEstado.disabled = true;
        ayudaEstado.textContent = "Las propiedades nuevas se cargan como Disponibles.";

        $("titulo-dialogo-inmueble").textContent = "Nueva propiedad";
        $("boton-guardar-inmueble").textContent = "Dar de alta";
        dialogo.showModal();
    }

    async function abrirEdicion(id) {
        ocultar(mensaje);
        const respuesta = await fetch(API + "/propiedades/" + id);
        if (!respuesta.ok) {
            mostrar(mensaje, await textoDeError(respuesta), "error");
            return;
        }
        const i = await respuesta.json();

        form.reset();
        $("inm-codigo").value = i.codigo;
        $("inm-titulo").value = i.titulo;
        $("inm-descripcion").value = i.descripcion || "";
        $("inm-sup-total").value = i.superficieTotal;
        $("inm-sup-cubierta").value = i.superficieCubierta === null ? "" : i.superficieCubierta;
        $("inm-ambientes").value = i.ambientes === null ? "" : i.ambientes;
        $("inm-dormitorios").value = i.dormitorios === null ? "" : i.dormitorios;
        $("inm-banios").value = i.banios === null ? "" : i.banios;
        $("inm-cochera").checked = i.cochera;
        $("inm-direccion").value = i.idDireccion;
        $("inm-tipo").value = i.idTipoInmueble;
        $("inm-inmobiliaria").value = i.idInmobiliaria;

        // En la edición sí se puede cambiar el estado: acá es donde una propiedad
        // Disponible pasa a Vendida, Alquilada o Reservada.
        selectEstado.disabled = false;
        selectEstado.value = i.idEstadoInmueble;
        ayudaEstado.textContent = i.idEstadoInmueble === catalogos.estadoDisponibleId
            ? "Cambiá el estado para concretar la operación. Al dejar de estar Disponible se dan de baja sus publicaciones."
            : "";

        idEditando = id;
        estadoAnterior = i.idEstadoInmueble;
        ocultar(mensajeDialogo);
        $("titulo-dialogo-inmueble").textContent = "Editar propiedad " + i.codigo;
        $("boton-guardar-inmueble").textContent = "Guardar cambios";
        dialogo.showModal();
    }

    function mensajeDeExito(editando, cuerpoEnviado) {
        if (!editando) {
            return "Propiedad creada como Disponible.";
        }

        const dejoDeEstarDisponible =
            estadoAnterior === catalogos.estadoDisponibleId &&
            cuerpoEnviado.idEstadoInmueble !== catalogos.estadoDisponibleId;

        if (dejoDeEstarDisponible) {
            return "Propiedad marcada como " + textoEstado(cuerpoEnviado.idEstadoInmueble) +
                ". Se dieron de baja sus publicaciones activas.";
        }

        return "Propiedad actualizada correctamente.";
    }

    // CREATE / UPDATE
    form.addEventListener("submit", async function (evento) {
        evento.preventDefault();
        ocultar(mensajeDialogo);

        const editando = idEditando !== null;
        const url = editando ? API + "/propiedades/" + idEditando : API + "/propiedades";
        const cuerpoEnviado = leerFormulario();

        try {
            const respuesta = await fetch(url, {
                method: editando ? "PUT" : "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(cuerpoEnviado)
            });

            if (!respuesta.ok) {
                mostrar(mensajeDialogo, await textoDeError(respuesta), "error");
                return;
            }

            const texto = mensajeDeExito(editando, cuerpoEnviado);
            dialogo.close();
            mostrar(mensaje, texto, "ok");
            await cargarListado();
            document.dispatchEvent(new CustomEvent("propiedades-actualizadas"));
        } catch (error) {
            mostrar(mensajeDialogo, "No se pudo conectar con el servidor.", "error");
            console.error(error);
        }
    });

    $("boton-cancelar-inmueble").addEventListener("click", function () { dialogo.close(); });
    dialogo.addEventListener("close", function () {
        idEditando = null;
        estadoAnterior = null;
        selectEstado.disabled = false;
    });

    // ---------- DELETE ----------
    function pedirConfirmacion(id, codigo) {
        pendienteEliminar = { id: id, codigo: codigo };
        $("texto-eliminar").textContent =
            "¿Seguro que querés eliminar la propiedad " + codigo + "? Esta acción no se puede deshacer.";
        dialogoEliminar.showModal();
    }

    $("boton-cancelar-eliminar").addEventListener("click", function () {
        pendienteEliminar = null;
        dialogoEliminar.close();
    });

    $("boton-confirmar-eliminar").addEventListener("click", async function () {
        if (!pendienteEliminar) return;
        const objetivo = pendienteEliminar;
        pendienteEliminar = null;
        dialogoEliminar.close();
        ocultar(mensaje);

        try {
            const respuesta = await fetch(API + "/propiedades/" + objetivo.id, { method: "DELETE" });
            if (!respuesta.ok) {
                mostrar(mensaje, await textoDeError(respuesta), "error");
                return;
            }
            mostrar(mensaje, "Propiedad " + objetivo.codigo + " eliminada.", "ok");
            await cargarListado();
            document.dispatchEvent(new CustomEvent("propiedades-actualizadas"));
        } catch (error) {
            mostrar(mensaje, "No se pudo conectar con el servidor.", "error");
            console.error(error);
        }
    });

    // ---------- Eventos generales e inicio ----------
    $("boton-nueva-inactiva").addEventListener("click", abrirAlta);

    formFiltros.addEventListener("submit", function (evento) {
        evento.preventDefault();
        ocultar(mensaje);
        cargarListado();
    });

    (async function iniciar() {
        try {
            await cargarCatalogos();
            await cargarListado();
        } catch (error) {
            mostrar(mensaje, "No se pudo conectar con el servidor.", "error");
            console.error(error);
        }
    })();
})();
