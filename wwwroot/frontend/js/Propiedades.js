
function verDetalles(id) {
    window.location.href = "Detalle.html?id=" + id;
}

const formulario = document.getElementById("formulario-busqueda");
const textoBusqueda = document.getElementById("texto-busqueda");

const propiedades = document.querySelectorAll(".tarjeta-propiedad");

formulario.addEventListener("submit", function(evento) {

    evento.preventDefault();

    const busqueda = textoBusqueda.value.toLowerCase().trim();

    propiedades.forEach(function(propiedad) {

        const informacion = propiedad.textContent.toLowerCase();

        if (informacion.includes(busqueda)) {
            propiedad.style.display = "";
        } else {
            propiedad.style.display = "none";
        }

    });

});
