// Cards de inmobiliarias con sus metricas de cartera.
// API: GET /Reporte/inmobiliarias
(function () {
    "use strict";

    const $ = function (id) { return document.getElementById(id); };

    const grilla = $("grilla-inmobiliarias");
    const mensaje = $("mensaje-inmobiliarias");
    const totales = $("totales-inmobiliarias");

    function numero(valor) {
        return Number(valor || 0).toLocaleString("es-AR");
    }

    function mostrarMensaje(texto, tipo) {
        mensaje.textContent = texto;
        mensaje.className = "crud-mensaje " + (tipo || "");
        mensaje.hidden = false;
    }

    function agregar(padre, etiqueta, texto, clase) {
        const elemento = document.createElement(etiqueta);
        elemento.textContent = texto;
        if (clase) elemento.className = clase;
        padre.appendChild(elemento);
        return elemento;
    }

    function metrica(padre, valor, etiqueta, clase) {
        const caja = document.createElement("div");
        caja.className = "inmo-metrica " + (clase || "");
        agregar(caja, "span", numero(valor), "inmo-metrica-valor");
        agregar(caja, "span", etiqueta, "inmo-metrica-etiqueta");
        padre.appendChild(caja);
    }

    function dibujarTarjeta(inmobiliaria) {
        const tarjeta = document.createElement("article");
        tarjeta.className = "inmo-tarjeta";

        const encabezado = document.createElement("div");
        encabezado.className = "inmo-encabezado";
        agregar(encabezado, "h3", inmobiliaria.nombre);
        agregar(encabezado, "p", inmobiliaria.direccion + ", " + inmobiliaria.ciudad,
                "inmo-ubicacion");
        tarjeta.appendChild(encabezado);

        const contacto = document.createElement("ul");
        contacto.className = "inmo-contacto";
        if (inmobiliaria.cuit) agregar(contacto, "li", "CUIT " + inmobiliaria.cuit);
        if (inmobiliaria.telefono) agregar(contacto, "li", inmobiliaria.telefono);
        if (inmobiliaria.email) agregar(contacto, "li", inmobiliaria.email);
        tarjeta.appendChild(contacto);

        const metricas = document.createElement("div");
        metricas.className = "inmo-metricas";
        metrica(metricas, inmobiliaria.totalPropiedades, "Propiedades");
        metrica(metricas, inmobiliaria.publicacionesActivas, "Publicadas", "inmo-publicadas");
        metrica(metricas, inmobiliaria.vendidas, "Vendidas", "inmo-vendidas");
        metrica(metricas, inmobiliaria.alquiladas, "Alquiladas", "inmo-alquiladas");
        metrica(metricas, inmobiliaria.disponibles, "Disponibles", "inmo-disponibles");
        metrica(metricas, inmobiliaria.reservadas, "Reservadas", "inmo-reservadas");
        tarjeta.appendChild(metricas);

        // Barra de composicion de la cartera
        const total = inmobiliaria.totalPropiedades || 0;
        if (total > 0) {
            const barra = document.createElement("div");
            barra.className = "inmo-barra";
            barra.title = "Composición de la cartera";

            [["disponibles", inmobiliaria.disponibles],
             ["vendidas", inmobiliaria.vendidas],
             ["alquiladas", inmobiliaria.alquiladas],
             ["reservadas", inmobiliaria.reservadas]].forEach(function (par) {
                if (par[1] > 0) {
                    const tramo = document.createElement("span");
                    tramo.className = "inmo-tramo inmo-tramo-" + par[0];
                    tramo.style.width = (par[1] * 100 / total) + "%";
                    barra.appendChild(tramo);
                }
            });

            tarjeta.appendChild(barra);
        }

        const pie = document.createElement("a");
        pie.className = "crud-btn crud-btn-primario inmo-boton";
        pie.href = "Reportes.html";
        pie.textContent = "Ver propiedades en reportes";
        tarjeta.appendChild(pie);

        grilla.appendChild(tarjeta);
    }

    function dibujarTotales(lista) {
        const suma = function (clave) {
            return lista.reduce(function (acumulado, i) {
                return acumulado + (i[clave] || 0);
            }, 0);
        };

        totales.innerHTML =
            '<div class="rep-chip"><span class="rep-chip-etiqueta">Inmobiliarias</span>' +
            '<span class="rep-chip-valor">' + numero(lista.length) + '</span></div>' +
            '<div class="rep-chip"><span class="rep-chip-etiqueta">Propiedades</span>' +
            '<span class="rep-chip-valor">' + numero(suma("totalPropiedades")) + '</span></div>' +
            '<div class="rep-chip"><span class="rep-chip-etiqueta">Publicadas</span>' +
            '<span class="rep-chip-valor">' + numero(suma("publicacionesActivas")) + '</span></div>' +
            '<div class="rep-chip"><span class="rep-chip-etiqueta">Vendidas</span>' +
            '<span class="rep-chip-valor">' + numero(suma("vendidas")) + '</span></div>' +
            '<div class="rep-chip"><span class="rep-chip-etiqueta">Alquiladas</span>' +
            '<span class="rep-chip-valor">' + numero(suma("alquiladas")) + '</span></div>';
    }

    (async function iniciar() {
        try {
            const respuesta = await fetch("/Reporte/inmobiliarias");

            if (!respuesta.ok) {
                mostrarMensaje("No se pudieron cargar las inmobiliarias (" +
                               respuesta.status + ").", "error");
                return;
            }

            const lista = await respuesta.json();

            if (lista.length === 0) {
                mostrarMensaje("Todavía no hay inmobiliarias cargadas.", "");
                return;
            }

            dibujarTotales(lista);
            lista.forEach(dibujarTarjeta);

        } catch (error) {
            mostrarMensaje("No se pudo conectar con el servidor.", "error");
            console.error(error);
        }
    })();
})();
