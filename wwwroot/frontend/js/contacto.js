
const aviso = document.getElementById("aviso-contacto");
const formulario = document.getElementById("formulario-contacto-general");

formulario.addEventListener("submit", function(evento) {
    evento.preventDefault();

    const nombre = document.getElementById("nombre").value.trim();
    const correo = document.getElementById("correo").value.trim();
    const consulta = document.getElementById("consulta").value.trim();

    if (nombre === "" || correo === "" || consulta === "") {
        aviso.textContent = "Por favor, completá todos los campos.";
        aviso.className = "aviso error";
        return;
    }

    aviso.textContent = "Gracias " + nombre + ". Tu consulta fue validada correctamente.";
    aviso.className = "aviso exito";

    formulario.reset();
});
