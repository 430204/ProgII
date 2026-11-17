// Fotos de ejemplo por tipo de inmueble (la base de datos no guarda imagenes).
// Para usar fotos reales: copiarlas a imagenes/propiedades/ y cambiar estas listas.
(function () {
    "use strict";

    var CARPETA = "imagenes/propiedades/";
    var CASAS = ["casa-1.jpg", "casa-2.jpg", "depto-1.jpg"];
    var DEPTOS = ["depto-1.jpg", "casa-2.jpg", "casa-1.jpg"];

    // Devuelve la lista de fotos de una publicacion. La foto principal rota segun el id,
    // para que no todas las tarjetas muestren la misma imagen.
    window.imagenesPropiedad = function (tipo, id) {
        var t = (tipo || "").toLowerCase();
        var base = (t.indexOf("casa") !== -1 || t.indexOf("lote") !== -1) ? CASAS : DEPTOS;
        var inicio = Math.abs(Number(id) || 0) % base.length;
        var lista = [];
        for (var i = 0; i < base.length; i++) {
            lista.push(CARPETA + base[(inicio + i) % base.length]);
        }
        return lista;
    };
})();
