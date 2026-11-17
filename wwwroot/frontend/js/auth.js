
(function () {
    const token = sessionStorage.getItem("inmoweb_token") ||
        localStorage.getItem("inmoweb_token");

    // Rutas que NO requieren estar logueado
    const rutasPublicas = [
        "/frontend/Login.html",
        "/Login.html",
        "/frontend/Inicio.html",
        "/Inicio.html"
    ];

    const rutaActual = window.location.pathname;

    const esRutaPublica = rutasPublicas.some(r => rutaActual.toLowerCase().endsWith(r.toLowerCase()));

    if (!token && !esRutaPublica) {
        window.location.href = "/frontend/Login.html";
    }
})();