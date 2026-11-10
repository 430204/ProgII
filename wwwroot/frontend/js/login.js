

const formularioLogin = document.getElementById("formulario-login");
const mensajeLogin = document.getElementById("mensaje-login");

formularioLogin.addEventListener("submit", function(evento) {

    evento.preventDefault();

    const apellido = document.getElementById("apellido").value.trim();
    const dni = document.getElementById("dni").value.trim();

    mensajeLogin.className = "";

    if (apellido === "" || dni === "") {
        mensajeLogin.textContent = "Completá todos los campos.";
        mensajeLogin.className = "mensaje-error";
        return;
    }

    if (!/^\d{7,8}$/.test(dni)) {
        mensajeLogin.textContent = "El DNI debe tener 7 u 8 números.";
        mensajeLogin.className = "mensaje-error";
        return;
    }

    mensajeLogin.textContent = "Datos validados. La autenticación todavía no está conectada.";
    mensajeLogin.className = "mensaje-exito";

});
async function iniciarSesion(apellido, dni) {

    const respuesta = await fetch("/api/Auth", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            apellido: apellido,
            dni: dni
        })
    });

    return respuesta;
}
