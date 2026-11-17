(function () {
    "use strict";

    var CARPETA = "imagenes/propiedades/";
    var CASAS = ["casa-1.jpg", "casa-2.jpg", "depto-1.jpg"];
    var DEPTOS = ["depto-1.jpg", "casa-2.jpg", "casa-1.jpg"];

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
