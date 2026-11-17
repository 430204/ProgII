// Login contra la API: POST /api/Auth devuelve un JWT que se guarda en sessionStorage.

const formularioLogin = document.getElementById("formulario-login");
const mensajeLogin = document.getElementById("mensaje-login");
const botonLogin = formularioLogin.querySelector("button[type=submit]");

const CLAVE_TOKEN = "inmoweb_token";
const CLAVE_USUARIO = "inmoweb_usuario";

function mostrarMensaje(texto, tipo) {
    mensajeLogin.textContent = texto;
    mensajeLogin.className = tipo === "error" ? "mensaje-error" : "mensaje-exito";
}

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

async function textoDeError(respuesta) {
    try {
        const datos = await respuesta.json();
        if (datos.mensaje) return datos.mensaje;
        if (datos.errors) return Object.values(datos.errors).flat().join(" ");
        if (datos.title) return datos.title;
    } catch (e) {
        // la respuesta no trae JSON
    }
    return "No se pudo iniciar sesión (" + respuesta.status + ").";
}

formularioLogin.addEventListener("submit", async function (evento) {

    evento.preventDefault();

    const apellido = document.getElementById("apellido").value.trim();
    const dni = document.getElementById("dni").value.trim();

    mensajeLogin.className = "";

    if (apellido === "" || dni === "") {
        mostrarMensaje("Completá todos los campos.", "error");
        return;
    }

    if (!/^\d{7,8}$/.test(dni)) {
        mostrarMensaje("El DNI debe tener 7 u 8 números.", "error");
        return;
    }

    botonLogin.disabled = true;
    mostrarMensaje("Verificando datos...", "exito");

    try {
        const respuesta = await iniciarSesion(apellido, dni);

        if (!respuesta.ok) {
            mostrarMensaje(await textoDeError(respuesta), "error");
            return;
        }

        const datos = await respuesta.json();

        sessionStorage.setItem(CLAVE_TOKEN, datos.token);
        sessionStorage.setItem(CLAVE_USUARIO, datos.usuario);

        mostrarMensaje("Bienvenido/a, " + datos.usuario + ". Redirigiendo...", "exito");

        setTimeout(function () {
            window.location.href = "Indice.html";
        }, 900);

    } catch (error) {
        mostrarMensaje("No se pudo conectar con el servidor.", "error");
        console.error(error);
    } finally {
        botonLogin.disabled = false;
    }
});
