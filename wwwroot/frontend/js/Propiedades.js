const formulario = document.getElementById("formulario-busqueda");
const textoBusqueda = document.getElementById("texto-busqueda");
const contenedor = document.getElementById("lista-inmuebles");

let publicaciones = [];

async function cargarPublicaciones() {
    const operacion = document.getElementById("operacion").value;
    const moneda = document.getElementById("moneda").value;
    const precioMaximo = document.getElementById("precio-maximo").value;

    if (precioMaximo !== "" && moneda === "") {
        alert("Seleccioná una moneda para filtrar por precio.");
        return;
    }

    const parametros = new URLSearchParams();

    if (operacion !== "") {
        parametros.append("operacion", operacion);
    }

    if (moneda !== "") {
        parametros.append("moneda", moneda);
    }

    if (precioMaximo !== "") {
        parametros.append("precioMaximo", precioMaximo);
    }
    mostrarTabla([]);
    contenedor.textContent = "Cargando propiedades...";

    try {
        const respuesta = await fetch(
            "/Inmueble/publicaciones-activas?" + parametros.toString()
        );

        if (!respuesta.ok) {
            contenedor.textContent =
                "No se pudo realizar la búsqueda. Revisá los filtros.";
            return;
        }

        publicaciones = await respuesta.json();

        const busqueda = textoBusqueda.value.toLowerCase().trim();

        const resultados = publicaciones.filter(function (publicacion) {
            const texto = publicacion.titulo + " "
                + publicacion.ciudad + " "
                + publicacion.tipoInmueble + " "
                + publicacion.operacion;

            return texto.toLowerCase().includes(busqueda);
        });

        mostrarPublicaciones(resultados);
    } catch (error) {
        contenedor.textContent =
            "No se pudo conectar con el servidor.";
        console.error(error);
    }
}

function agregarTexto(contenedorTexto, etiqueta, texto) {
    const elemento = document.createElement(etiqueta);
    elemento.textContent = texto;
    contenedorTexto.appendChild(elemento);
}

function mostrarPublicaciones(lista) {
    mostrarTabla(lista);
    contenedor.innerHTML = "";

    if (lista.length === 0) {
        contenedor.textContent = "No se encontraron propiedades.";
        return;
    }

    lista.forEach(function (publicacion) {
        const tarjeta = document.createElement("article");
        tarjeta.className = "tarjeta-propiedad";

        const informacion = document.createElement("div");
        informacion.className = "informacion-propiedad";

        agregarTexto(informacion, "h3", publicacion.titulo);

        agregarTexto(
            informacion,
            "p",
            publicacion.tipoInmueble + " - " + publicacion.operacion
        );

        agregarTexto(
            informacion,
            "p",
            publicacion.calle + " " + publicacion.numero
            + ", " + publicacion.ciudad
        );

        agregarTexto(
            informacion,
            "h4",
            publicacion.moneda + " "
            + publicacion.precio.toLocaleString("es-AR")
        );

        tarjeta.appendChild(informacion);
        contenedor.appendChild(tarjeta);
    });
}

formulario.addEventListener("submit", function (evento) {
    evento.preventDefault();
    cargarPublicaciones();
});

function mostrarTabla(lista) {
    const cuerpo = document.getElementById("cuerpo-resultados");
    const cantidad = document.getElementById("cantidad-resultados");

    cuerpo.innerHTML = "";
    cantidad.textContent = "Publicaciones encontradas: " + lista.length;

    lista.forEach(function (publicacion) {
        const fila = document.createElement("tr");

        agregarTexto(fila, "td", publicacion.codigo);
        agregarTexto(fila, "td", publicacion.titulo);
        agregarTexto(fila, "td", publicacion.tipoInmueble);
        agregarTexto(fila, "td", publicacion.ciudad);
        agregarTexto(fila, "td", publicacion.operacion);
        agregarTexto(fila, "td", publicacion.moneda);

        agregarTexto(
            fila,
            "td",
            publicacion.precio.toLocaleString("es-AR", {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            })
        );

        cuerpo.appendChild(fila);
    });
}

cargarPublicaciones();