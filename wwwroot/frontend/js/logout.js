document.addEventListener("DOMContentLoaded", function () {

    const botonCerrarSesion = document.getElementById("btnCerrarSesion");

    if (botonCerrarSesion) {

        botonCerrarSesion.addEventListener("click", function (e) {

            e.preventDefault();

            sessionStorage.removeItem("inmoweb_token");
            localStorage.removeItem("inmoweb_token");

            window.location.href = "/frontend/Inicio.html";

        });

    }

});