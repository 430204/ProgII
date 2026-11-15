
(function () {
    const token = localStorage.getItem("token") || sessionStorage.getItem("token");

    // Rutas que NO requieren estar logueado
    const rutasPublicas = [
        "/frontend/Login.html",
        "/Login.html"
    ];

    const rutaActual = window.location.pathname;

    const esRutaPublica = rutasPublicas.some(r => rutaActual.toLowerCase().endsWith(r.toLowerCase()));

    if (!token && !esRutaPublica) {
        window.location.href = "/frontend/Login.html";
    }
})();