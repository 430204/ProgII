
const propiedades = [
    {
        nombre: "Casa en Nueva Córdoba",
        precio: "USD 150.000",
        ubicacion: "Córdoba Capital",
        caracteristicas: "3 dormitorios - 2 baños",
        imagen: "imagenes/casa1.jpg"
    },
    {
        nombre: "Departamento en General Paz",
        precio: "USD 95.000",
        ubicacion: "Córdoba Capital",
        caracteristicas: "2 dormitorios - 1 baño",
        imagen: "imagenes/dto1.jpg"
    },
    {
        nombre: "Casa en Villa Allende",
        precio: "USD 220.000",
        ubicacion: "Villa Allende, Córdoba",
        caracteristicas: "4 dormitorios - 3 baños",
        imagen: "imagenes/casa2.jpg"
    }
];

const parametros = new URLSearchParams(window.location.search);
const id = Number(parametros.get("id"));

const propiedad = propiedades[id];
console.log("ID recibido:", id);
console.log("Propiedad encontrada:", propiedad);

if (propiedad) {
    document.getElementById("nombre-propiedad").textContent = propiedad.nombre;
    document.getElementById("precio-propiedad").textContent = propiedad.precio;
    document.getElementById("ubicacion-propiedad").textContent = propiedad.ubicacion;
    document.getElementById("caracteristicas-propiedad").textContent = propiedad.caracteristicas;
    document.getElementById("imagen-detalle").src = propiedad.imagen;
} else {
    document.getElementById("nombre-propiedad").textContent =
        "Propiedad no encontrada";
}

const formularioContacto = document.getElementById("formulario-contacto");

formularioContacto.addEventListener("submit", function(evento) {
    evento.preventDefault();

    const nombre = document.getElementById("nombre-contacto").value.trim();
    const email = document.getElementById("email-contacto").value.trim();
    const mensaje = document.getElementById("mensaje-contacto").value.trim();

    if (nombre === "" || email === "" || mensaje === "") {
        alert("Por favor, completá todos los campos.");
        return;
    }

    alert("Gracias " + nombre + ". Tu consulta fue preparada correctamente.");

    formularioContacto.reset();
});

