const parametros = new URLSearchParams(window.location.search);
const idPublicacion = Number(parametros.get("id"));

function texto(id, valor) {
    document.getElementById(id).textContent = valor;
}

function agregarCaracteristica(lista, etiqueta, valor) {
    if (valor === null || valor === undefined || valor === "") return;
    const item = document.createElement("li");
    const nombre = document.createElement("span");
    nombre.textContent = etiqueta;
    const dato = document.createElement("strong");
    dato.textContent = valor;
    item.appendChild(nombre);
    item.appendChild(dato);
    lista.appendChild(item);
}

function mostrarGaleria(fotos, titulo) {
    const principal = document.getElementById("imagen-detalle");
    const miniaturas = document.getElementById("miniaturas");
    miniaturas.innerHTML = "";

    function elegir(indice) {
        principal.src = fotos[indice];
        principal.alt = "Foto " + (indice + 1) + " de " + titulo;
        miniaturas.querySelectorAll("img").forEach(function (m, j) {
            m.classList.toggle("activa", j === indice);
        });
    }

    fotos.forEach(function (ruta, indice) {
        const mini = document.createElement("img");
        mini.src = ruta;
        mini.alt = "Miniatura " + (indice + 1);
        mini.addEventListener("click", function () { elegir(indice); });
        miniaturas.appendChild(mini);
    });
    elegir(0);
}

function metros(valor) {
    return valor === null || valor === undefined ? null : Number(valor).toLocaleString("es-AR") + " m²";
}

async function cargarDetalle() {
    if (!idPublicacion) {
        texto("nombre-propiedad", "Propiedad no encontrada");
        return;
    }

    try {
        const respuesta = await fetch("/Inmueble/publicaciones/" + idPublicacion);

        if (respuesta.status === 404) {
            texto("nombre-propiedad", "Propiedad no encontrada");
            texto("subtitulo-propiedad", "La publicación no existe o ya no está activa.");
            return;
        }
        if (!respuesta.ok) {
            texto("nombre-propiedad", "No se pudo cargar la propiedad");
            return;
        }

        const p = await respuesta.json();
        document.title = p.titulo + " - InmoWeb";

        texto("nombre-propiedad", p.titulo);
        texto("subtitulo-propiedad", p.tipoInmueble + " en " + p.operacion.toLowerCase() + " · Código " + p.codigo);
        texto("precio-propiedad", p.moneda + " " + p.precio.toLocaleString("es-AR"));
        texto("ubicacion-propiedad", "📍 " + p.calle + " " + p.numero + ", " + p.ciudad);

        mostrarGaleria(window.imagenesPropiedad(p.tipoInmueble, p.idPublicacion), p.titulo);

        const lista = document.getElementById("caracteristicas-propiedad");
        lista.innerHTML = "";
        agregarCaracteristica(lista, "Superficie total", metros(p.superficieTotal));
        agregarCaracteristica(lista, "Superficie cubierta", metros(p.superficieCubierta));
        agregarCaracteristica(lista, "Ambientes", p.ambientes);
        agregarCaracteristica(lista, "Dormitorios", p.dormitorios);
        agregarCaracteristica(lista, "Baños", p.banios);
        agregarCaracteristica(lista, "Cochera", p.cochera ? "Sí" : "No");
        agregarCaracteristica(lista, "Publicada el", new Date(p.fechaPublicacion + "T00:00:00").toLocaleDateString("es-AR"));

        if (p.descripcion) {
            texto("descripcion-propiedad", p.descripcion);
            document.getElementById("bloque-descripcion").hidden = false;
        }

        const contacto = [p.inmobiliaria, p.inmobiliariaTelefono, p.inmobiliariaEmail]
            .filter(function (x) { return x; }).join(" · ");
        if (contacto) {
            texto("inmobiliaria-propiedad", contacto);
            document.getElementById("bloque-inmobiliaria").hidden = false;
        }
    } catch (error) {
        texto("nombre-propiedad", "No se pudo conectar con el servidor");
        console.error(error);
    }
}

cargarDetalle();

const avisoDetalle = document.getElementById("aviso-detalle");
const formularioContacto = document.getElementById("formulario-contacto");

formularioContacto.addEventListener("submit", function(evento) {
    evento.preventDefault();

    const nombre = document.getElementById("nombre-contacto").value.trim();
    const email = document.getElementById("email-contacto").value.trim();
    const mensaje = document.getElementById("mensaje-contacto").value.trim();

    if (nombre === "" || email === "" || mensaje === "") {
        avisoDetalle.textContent = "Por favor, completá todos los campos.";
        avisoDetalle.className = "aviso error";
        return;
    }

    avisoDetalle.textContent = "Gracias " + nombre + ". Tu consulta fue preparada correctamente.";
    avisoDetalle.className = "aviso exito";

    formularioContacto.reset();
});

