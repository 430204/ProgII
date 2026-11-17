// Pantalla de reportes. Cada reporte arma su query string con los filtros que
// el usuario carga en pantalla y pega contra /Reporte/...
(function () {
    "use strict";

    const $ = function (id) { return document.getElementById(id); };

    let filtros = null;          // respuesta de /Reporte/filtros
    const yaGenerado = {};       // reportes que ya se corrieron al menos una vez

    // ---------------------------------------------------------------- formato
    function formatearNumero(valor, decimales) {
        if (valor === null || valor === undefined) return "-";
        return Number(valor).toLocaleString("es-AR", {
            minimumFractionDigits: decimales === undefined ? 2 : decimales,
            maximumFractionDigits: decimales === undefined ? 2 : decimales
        });
    }

    function formatearFecha(valor) {
        if (!valor) return "-";
        const partes = String(valor).substring(0, 10).split("-");
        if (partes.length !== 3) return valor;
        return partes[2] + "/" + partes[1] + "/" + partes[0];
    }

    function claseEstado(estado) {
        const e = (estado || "").toLowerCase();
        if (e === "disponible" || e === "pagada") return "crud-badge crud-badge-disponible";
        if (e === "vendido") return "crud-badge crud-badge-vendido";
        if (e === "alquilado" || e === "vigente") return "crud-badge crud-badge-alquilado";
        if (e === "reservado" || e === "pendiente") return "crud-badge crud-badge-reservado";
        return "crud-badge";
    }

    // ---------------------------------------------------------------- helpers
    function valor(id) {
        const elemento = $(id);
        const v = elemento.value.trim();
        return v === "" ? null : v;
    }

    function queryString(parametros) {
        const partes = new URLSearchParams();
        Object.keys(parametros).forEach(function (clave) {
            const v = parametros[clave];
            if (v !== null && v !== undefined && v !== "") {
                partes.append(clave, v);
            }
        });
        const texto = partes.toString();
        return texto === "" ? "" : "?" + texto;
    }

    async function textoDeError(respuesta) {
        try {
            const datos = await respuesta.json();
            if (datos.mensaje) return datos.mensaje;
            if (datos.errors) return Object.values(datos.errors).flat().join(" ");
            if (datos.title) return datos.title;
        } catch (e) { /* sin cuerpo JSON */ }
        return "No se pudo generar el reporte (" + respuesta.status + ").";
    }

    function mostrarMensaje(id, texto, tipo) {
        const elemento = $(id);
        elemento.textContent = texto;
        elemento.className = "crud-mensaje " + (tipo || "");
        elemento.hidden = false;
    }

    function ocultarMensaje(id) {
        const elemento = $(id);
        elemento.textContent = "";
        elemento.hidden = true;
    }

    function llenarSelect(id, opciones, textoVacio) {
        const select = $(id);
        select.innerHTML = "";
        const vacio = document.createElement("option");
        vacio.value = "";
        vacio.textContent = textoVacio;
        select.appendChild(vacio);
        (opciones || []).forEach(function (texto) {
            const opcion = document.createElement("option");
            opcion.value = texto;
            opcion.textContent = texto;
            select.appendChild(opcion);
        });
    }

    // ------------------------------------------------- definicion de reportes
    const REPORTES = {
        busqueda: {
            endpoint: "/Reporte/busqueda-inmuebles",
            columnas: 8,
            parametros: function () {
                return {
                    operacion: valor("busqueda-operacion"),
                    tipo: valor("busqueda-tipo"),
                    ciudad: valor("busqueda-ciudad"),
                    precioMin: valor("busqueda-precio-min"),
                    precioMax: valor("busqueda-precio-max")
                };
            },
            fila: function (f) {
                const direccion = f.calle + " " + f.numero +
                    (f.piso ? ", piso " + f.piso : "") +
                    (f.departamento ? " " + f.departamento : "");
                return [
                    f.codigo, f.titulo, f.tipoInmueble, f.operacion,
                    f.ciudad, direccion, f.moneda,
                    { texto: formatearNumero(f.precio), derecha: true }
                ];
            }
        },

        publicaciones: {
            endpoint: "/Reporte/publicaciones",
            columnas: 8,
            parametros: function () {
                return {
                    desde: valor("pub-desde"),
                    hasta: valor("pub-hasta"),
                    operacion: valor("pub-operacion"),
                    tipo: valor("pub-tipo"),
                    ciudad: valor("pub-ciudad"),
                    moneda: valor("pub-moneda"),
                    precioMax: valor("pub-precio-max")
                };
            },
            fila: function (f) {
                return [
                    f.codigo, f.titulo, f.tipoInmueble, f.operacion, f.ciudad,
                    formatearFecha(f.fechaPublicacion), f.moneda,
                    { texto: formatearNumero(f.precio), derecha: true }
                ];
            }
        },

        contratos: {
            endpoint: "/Reporte/contratos",
            columnas: 12,
            parametros: function () {
                return {
                    desde: valor("con-desde"),
                    hasta: valor("con-hasta"),
                    operacion: valor("con-operacion"),
                    estado: valor("con-estado")
                };
            },
            fila: function (f) {
                return [
                    f.codigo, f.titulo, f.operacion, f.cliente, f.agente,
                    formatearFecha(f.fechaInicio),
                    f.fechaFin ? formatearFecha(f.fechaFin) : "-",
                    f.moneda,
                    { texto: formatearNumero(f.montoAcordado), derecha: true },
                    { texto: formatearNumero(f.comisionPorcentaje) + " %", derecha: true },
                    { texto: formatearNumero(f.comisionCalculada), derecha: true },
                    { texto: f.estado, badge: true }
                ];
            }
        },

        facturacion: {
            endpoint: "/Reporte/facturacion",
            columnas: 8,
            parametros: function () {
                return {
                    desde: valor("fac-desde"),
                    hasta: valor("fac-hasta"),
                    estado: valor("fac-estado")
                };
            },
            fila: function (f) {
                return [
                    f.numeroFactura, f.codigo, f.cliente,
                    formatearFecha(f.fechaEmision),
                    f.fechaVencimiento ? formatearFecha(f.fechaVencimiento) : "-",
                    { texto: f.estado, badge: true },
                    f.moneda,
                    { texto: formatearNumero(f.total), derecha: true }
                ];
            }
        },

        resumen: {
            endpoint: "/Reporte/resumen",
            columnas: 5,
            parametros: function () {
                return {
                    ciudad: valor("res-ciudad"),
                    tipo: valor("res-tipo")
                };
            },
            fila: function (f) {
                return [
                    f.ciudad, f.tipoInmueble,
                    { texto: formatearNumero(f.cantidadInmuebles, 0), derecha: true },
                    { texto: formatearNumero(f.disponibles, 0), derecha: true },
                    { texto: formatearNumero(f.noDisponibles, 0), derecha: true }
                ];
            }
        }
    };

    // ---------------------------------------------------------------- dibujo
    function dibujarTabla(nombre, filas) {
        const definicion = REPORTES[nombre];
        const cuerpo = $("tabla-" + nombre).querySelector("tbody");
        cuerpo.innerHTML = "";

        if (filas.length === 0) {
            const tr = document.createElement("tr");
            const td = document.createElement("td");
            td.colSpan = definicion.columnas;
            td.className = "crud-vacio";
            td.textContent = "El reporte no devolvió filas con esos filtros.";
            tr.appendChild(td);
            cuerpo.appendChild(tr);
            return;
        }

        filas.forEach(function (f) {
            const tr = document.createElement("tr");
            definicion.fila(f).forEach(function (celda) {
                const td = document.createElement("td");

                if (celda !== null && typeof celda === "object") {
                    if (celda.badge) {
                        const span = document.createElement("span");
                        span.className = claseEstado(celda.texto);
                        span.textContent = celda.texto;
                        td.appendChild(span);
                    } else {
                        td.textContent = celda.texto;
                    }
                    if (celda.derecha) td.className = "rep-der";
                } else {
                    td.textContent = celda === null || celda === undefined ? "" : celda;
                }

                tr.appendChild(td);
            });
            cuerpo.appendChild(tr);
        });
    }

    function chip(etiqueta, valorTexto) {
        return '<div class="rep-chip"><span class="rep-chip-etiqueta">' + etiqueta +
               '</span><span class="rep-chip-valor">' + valorTexto + '</span></div>';
    }

    function dibujarTotales(nombre, datos) {
        const contenedor = $("totales-" + nombre);
        let html = chip("Filas", formatearNumero(datos.cantidad, 0));

        if (nombre === "facturacion") {
            html += chip("Pagadas", formatearNumero(datos.pagadas, 0));
            html += chip("Pendientes", formatearNumero(datos.pendientes, 0));
        }

        if (nombre === "resumen") {
            html += chip("Total inmuebles", formatearNumero(datos.totalInmuebles, 0));
            html += chip("Total disponibles", formatearNumero(datos.totalDisponibles, 0));
        }

        (datos.totales || []).forEach(function (t) {
            html += chip(t.moneda + " · " + t.cantidad + " reg.",
                         t.moneda + " " + formatearNumero(t.monto));
            if (t.comision !== null && t.comision !== undefined) {
                html += chip("Comisiones " + t.moneda,
                             t.moneda + " " + formatearNumero(t.comision));
            }
            if (t.promedio !== null && t.promedio !== undefined) {
                html += chip("Promedio " + t.moneda,
                             t.moneda + " " + formatearNumero(t.promedio));
            }
        });

        contenedor.innerHTML = html;
    }

    // ---------------------------------------------------------------- generar
    async function generar(nombre) {
        const definicion = REPORTES[nombre];
        ocultarMensaje("mensaje-" + nombre);

        try {
            const url = definicion.endpoint + queryString(definicion.parametros());
            const respuesta = await fetch(url);

            if (!respuesta.ok) {
                mostrarMensaje("mensaje-" + nombre, await textoDeError(respuesta), "error");
                dibujarTabla(nombre, []);
                $("totales-" + nombre).innerHTML = "";
                return;
            }

            const datos = await respuesta.json();
            dibujarTabla(nombre, datos.filas || []);
            dibujarTotales(nombre, datos);
            yaGenerado[nombre] = true;

        } catch (error) {
            mostrarMensaje("mensaje-" + nombre, "No se pudo conectar con el servidor.", "error");
            console.error(error);
        }
    }

    // ---------------------------------------------------------------- pestanias
    function mostrarPanel(nombre) {
        document.querySelectorAll(".rep-pestania").forEach(function (boton) {
            boton.classList.toggle("activa", boton.dataset.reporte === nombre);
        });
        document.querySelectorAll(".rep-panel").forEach(function (panel) {
            panel.hidden = panel.id !== "panel-" + nombre;
        });

        // La primera vez que se abre una pestania se genera sola, para que
        // nunca se vea una tabla vacia sin haber tocado nada.
        if (!yaGenerado[nombre]) {
            generar(nombre);
        }
    }

    document.querySelectorAll(".rep-pestania").forEach(function (boton) {
        boton.addEventListener("click", function () {
            mostrarPanel(boton.dataset.reporte);
        });
    });

    Object.keys(REPORTES).forEach(function (nombre) {
        const form = $("form-" + nombre);

        form.addEventListener("submit", function (evento) {
            evento.preventDefault();
            generar(nombre);
        });

        form.addEventListener("reset", function () {
            // El reset del navegador corre despues del evento, por eso el setTimeout
            setTimeout(function () { generar(nombre); }, 0);
        });
    });

    // ---------------------------------------------------------------- inicio
    async function cargarFiltros() {
        const respuesta = await fetch("/Reporte/filtros");
        if (!respuesta.ok) throw new Error("No se pudieron cargar los filtros.");
        filtros = await respuesta.json();

        llenarSelect("busqueda-operacion", filtros.operaciones, "Todas");
        llenarSelect("busqueda-tipo", filtros.tipos, "Todos");
        llenarSelect("busqueda-ciudad", filtros.ciudades, "Todas");

        llenarSelect("pub-operacion", filtros.operaciones, "Todas");
        llenarSelect("pub-tipo", filtros.tipos, "Todos");
        llenarSelect("pub-ciudad", filtros.ciudades, "Todas");
        llenarSelect("pub-moneda", filtros.monedas, "Todas");

        llenarSelect("con-operacion", filtros.operaciones, "Todas");
        llenarSelect("con-estado", filtros.estadosContrato, "Todos");

        llenarSelect("fac-estado", filtros.estadosFactura, "Todos");

        llenarSelect("res-ciudad", filtros.ciudades, "Todas");
        llenarSelect("res-tipo", filtros.tipos, "Todos");

        // Los rangos de fecha arrancan cubriendo todo lo que hay cargado,
        // asi el reporte devuelve filas apenas se abre.
        if (filtros.primeraPublicacion) $("pub-desde").value = filtros.primeraPublicacion;
        if (filtros.ultimaPublicacion) $("pub-hasta").value = filtros.ultimaPublicacion;
        if (filtros.primerContrato) $("con-desde").value = filtros.primerContrato;
        if (filtros.ultimoContrato) $("con-hasta").value = filtros.ultimoContrato;
    }

    (async function iniciar() {
        try {
            await cargarFiltros();
            mostrarPanel("busqueda");
        } catch (error) {
            mostrarMensaje("mensaje-busqueda",
                "No se pudo conectar con el servidor.", "error");
            console.error(error);
        }
    })();
})();
