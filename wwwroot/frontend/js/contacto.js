
const formulario = document.getElementById("formulario-contacto-general");

formulario.addEventListener("submit", function(evento) {
    evento.preventDefault();

    const nombre = document.getElementById("nombre").value.trim();
    const correo = document.getElementById("correo").value.trim();
    const consulta = document.getElementById("consulta").value.trim();

    if (nombre === "" || correo === "" || consulta === "") {
        alert("Por favor, completá todos los campos.");
        return;
    }

    alert("Gracias " + nombre + ". Tu consulta fue validada correctamente.");

    formulario.reset();
});
