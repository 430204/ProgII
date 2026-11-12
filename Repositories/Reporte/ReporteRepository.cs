using InmoWeb.DTOs;
using InmoWeb.Models;
using InmoWeb.Repositories.Reporte.Interfaz;
using Microsoft.EntityFrameworkCore;

namespace InmoWeb.Repositories.Reporte
{
    public class ReporteRepository : IReporteRepository
    {
        private readonly InmobiliariaDBContext db;

        public ReporteRepository(InmobiliariaDBContext db)
        {
            this.db = db;
        }

        // ------------------------------------------------------------------
        // Reporte 1: sp_BuscarInmuebles
        // El procedimiento resuelve la busqueda contra fn_InmueblesDisponibles,
        // asi que una sola llamada ejercita el SP y la funcion de tabla.
        // ------------------------------------------------------------------
        public async Task<List<BusquedaInmuebleDto>> BuscarInmueblesAsync(
            string? operacion, string? tipo, string? ciudad,
            decimal? precioMin, decimal? precioMax)
        {
            var resultado = await db.Procedures.sp_BuscarInmueblesAsync(
                operacion, tipo, ciudad, precioMin, precioMax);

            return resultado.Select(r => new BusquedaInmuebleDto
            {
                IdInmueble = r.id_inmueble,
                Codigo = r.codigo,
                Titulo = r.titulo,
                TipoInmueble = r.tipo_inmueble,
                Operacion = r.operacion,
                Precio = r.precio,
                Moneda = r.moneda,
                Ciudad = r.ciudad,
                Calle = r.calle,
                Numero = r.numero,
                Piso = r.piso,
                Departamento = r.departamento
            }).ToList();
        }

        // ------------------------------------------------------------------
        // Reporte 2: vw_PublicacionesActivas
        // ------------------------------------------------------------------
        public async Task<List<PublicacionReporteDto>> GetPublicacionesAsync(
            string? operacion, string? tipo, string? ciudad, string? moneda,
            decimal? precioMin, decimal? precioMax,
            DateOnly? desde, DateOnly? hasta)
        {
            var consulta = db.VwPublicacionesActivas.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(operacion))
                consulta = consulta.Where(p => p.Operacion == operacion);

            if (!string.IsNullOrWhiteSpace(tipo))
                consulta = consulta.Where(p => p.TipoInmueble == tipo);

            if (!string.IsNullOrWhiteSpace(ciudad))
                consulta = consulta.Where(p => p.Ciudad == ciudad);

            if (!string.IsNullOrWhiteSpace(moneda))
                consulta = consulta.Where(p => p.Moneda == moneda);

            if (precioMin.HasValue)
                consulta = consulta.Where(p => p.Precio >= precioMin.Value);

            if (precioMax.HasValue)
                consulta = consulta.Where(p => p.Precio <= precioMax.Value);

            if (desde.HasValue)
                consulta = consulta.Where(p => p.FechaPublicacion >= desde.Value);

            if (hasta.HasValue)
                consulta = consulta.Where(p => p.FechaPublicacion <= hasta.Value);

            return await consulta
                .OrderByDescending(p => p.FechaPublicacion)
                .ThenBy(p => p.Precio)
                .Select(p => new PublicacionReporteDto
                {
                    IdPublicacion = p.IdPublicacion,
                    Codigo = p.Codigo,
                    Titulo = p.Titulo,
                    Operacion = p.Operacion,
                    Precio = p.Precio,
                    Moneda = p.Moneda,
                    FechaPublicacion = p.FechaPublicacion,
                    Calle = p.Calle,
                    Numero = p.Numero,
                    Ciudad = p.Ciudad,
                    TipoInmueble = p.TipoInmueble
                })
                .ToListAsync();
        }

        // ------------------------------------------------------------------
        // Reporte 3: vw_ContratosDetalle
        // La vista ya trae comision_calculada, que sale de fn_ComisionContrato.
        // Se une con Contrato para recuperar la moneda, que la vista no expone.
        // ------------------------------------------------------------------
        public async Task<List<ContratoReporteDto>> GetContratosAsync(
            DateOnly? desde, DateOnly? hasta, string? estado, string? operacion)
        {
            var consulta = from v in db.VwContratosDetalles.AsNoTracking()
                           join c in db.Contratos.AsNoTracking()
                               on v.IdContrato equals c.IdContrato
                           select new { v, c };

            if (desde.HasValue)
                consulta = consulta.Where(x => x.v.FechaInicio >= desde.Value);

            if (hasta.HasValue)
                consulta = consulta.Where(x => x.v.FechaInicio <= hasta.Value);

            if (!string.IsNullOrWhiteSpace(estado))
                consulta = consulta.Where(x => x.v.Estado == estado);

            if (!string.IsNullOrWhiteSpace(operacion))
                consulta = consulta.Where(x => x.v.Operacion == operacion);

            return await consulta
                .OrderByDescending(x => x.v.FechaInicio)
                .Select(x => new ContratoReporteDto
                {
                    IdContrato = x.v.IdContrato,
                    Codigo = x.v.Codigo,
                    Titulo = x.v.Titulo,
                    Operacion = x.v.Operacion,
                    Cliente = x.v.Cliente,
                    Agente = x.v.Agente,
                    FechaInicio = x.v.FechaInicio,
                    FechaFin = x.v.FechaFin,
                    MontoAcordado = x.v.MontoAcordado,
                    ComisionPorcentaje = x.v.ComisionPorcentaje,
                    ComisionCalculada = x.v.ComisionCalculada,
                    Estado = x.v.Estado,
                    Moneda = x.c.IdPublicacionNavigation.Moneda
                })
                .ToListAsync();
        }

        // ------------------------------------------------------------------
        // Reporte 4: vw_FacturasTotales
        // El total sale de la vista, que agrupa DetalleFactura.
        // ------------------------------------------------------------------
        public async Task<List<FacturaReporteDto>> GetFacturasAsync(
            DateOnly? desde, DateOnly? hasta, string? estado)
        {
            var consulta = from v in db.VwFacturasTotales.AsNoTracking()
                           join f in db.Facturas.AsNoTracking()
                               on v.IdFactura equals f.IdFactura
                           select new { v, f };

            if (desde.HasValue)
                consulta = consulta.Where(x => x.v.FechaEmision >= desde.Value);

            if (hasta.HasValue)
                consulta = consulta.Where(x => x.v.FechaEmision <= hasta.Value);

            if (!string.IsNullOrWhiteSpace(estado))
                consulta = consulta.Where(x => x.v.Estado == estado);

            return await consulta
                .OrderByDescending(x => x.v.FechaEmision)
                .Select(x => new FacturaReporteDto
                {
                    IdFactura = x.v.IdFactura,
                    NumeroFactura = x.v.NumeroFactura,
                    IdContrato = x.v.IdContrato,
                    Codigo = x.f.IdContratoNavigation.IdPublicacionNavigation
                                 .IdInmuebleNavigation.Codigo,
                    Cliente = x.f.IdContratoNavigation.IdClienteNavigation.Nombre + " " +
                              x.f.IdContratoNavigation.IdClienteNavigation.Apellido,
                    FechaEmision = x.v.FechaEmision,
                    FechaVencimiento = x.v.FechaVencimiento,
                    Estado = x.v.Estado,
                    Total = x.v.Total,
                    Moneda = x.f.IdContratoNavigation.IdPublicacionNavigation.Moneda
                })
                .ToListAsync();
        }

        // ------------------------------------------------------------------
        // Reporte 5: vw_ResumenInmobiliaria (agrupada por ciudad y tipo)
        // ------------------------------------------------------------------
        public async Task<List<ResumenReporteDto>> GetResumenAsync(string? ciudad, string? tipo)
        {
            var consulta = db.VwResumenInmobiliaria.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(ciudad))
                consulta = consulta.Where(r => r.Ciudad == ciudad);

            if (!string.IsNullOrWhiteSpace(tipo))
                consulta = consulta.Where(r => r.TipoInmueble == tipo);

            return await consulta
                .OrderBy(r => r.Ciudad)
                .ThenBy(r => r.TipoInmueble)
                .Select(r => new ResumenReporteDto
                {
                    Ciudad = r.Ciudad,
                    TipoInmueble = r.TipoInmueble,
                    CantidadInmuebles = r.CantidadInmuebles ?? 0,
                    Disponibles = r.Disponibles ?? 0,
                    NoDisponibles = (r.CantidadInmuebles ?? 0) - (r.Disponibles ?? 0)
                })
                .ToListAsync();
        }

        // ------------------------------------------------------------------
        // Metricas por inmobiliaria.
        // No hay vista para esto: vw_ResumenInmobiliaria agrupa por ciudad y
        // tipo de inmueble, no por inmobiliaria, a pesar de su nombre.
        // ------------------------------------------------------------------
        public async Task<List<InmobiliariaResumenDto>> GetInmobiliariasAsync()
        {
            return await db.Inmobiliaria.AsNoTracking()
                .OrderBy(x => x.Nombre)
                .Select(x => new InmobiliariaResumenDto
                {
                    IdInmobiliaria = x.IdInmobiliaria,
                    Nombre = x.Nombre,
                    Cuit = x.Cuit,
                    Telefono = x.Telefono,
                    Email = x.Email,
                    Ciudad = x.IdDireccionNavigation.IdCiudadNavigation.Nombre,
                    Direccion = x.IdDireccionNavigation.Calle + " " +
                                x.IdDireccionNavigation.Numero,

                    TotalPropiedades = x.Inmuebles.Count(),
                    Disponibles = x.Inmuebles.Count(i =>
                        i.IdEstadoInmuebleNavigation.Descripcion == "Disponible"),
                    Vendidas = x.Inmuebles.Count(i =>
                        i.IdEstadoInmuebleNavigation.Descripcion == "Vendido"),
                    Alquiladas = x.Inmuebles.Count(i =>
                        i.IdEstadoInmuebleNavigation.Descripcion == "Alquilado"),
                    Reservadas = x.Inmuebles.Count(i =>
                        i.IdEstadoInmuebleNavigation.Descripcion == "Reservado"),
                    PublicacionesActivas = x.Inmuebles
                        .SelectMany(i => i.Publicacions)
                        .Count(p => p.Activa)
                })
                .ToListAsync();
        }

        // ------------------------------------------------------------------
        // Opciones de los desplegables, tomadas de los datos reales
        // ------------------------------------------------------------------
        public async Task<FiltrosReporteDto> GetFiltrosAsync()
        {
            var ciudades = await db.Ciudads.AsNoTracking()
                .Where(c => c.Direccions.Any(d => d.Inmuebles.Any()))
                .OrderBy(c => c.Nombre)
                .Select(c => c.Nombre)
                .ToListAsync();

            var tipos = await db.TipoInmuebles.AsNoTracking()
                .OrderBy(t => t.Descripcion)
                .Select(t => t.Descripcion)
                .ToListAsync();

            var operaciones = await db.TipoOperacions.AsNoTracking()
                .OrderBy(o => o.Descripcion)
                .Select(o => o.Descripcion)
                .ToListAsync();

            var monedas = await db.Publicacions.AsNoTracking()
                .Select(p => p.Moneda)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            var estadosContrato = await db.Contratos.AsNoTracking()
                .Select(c => c.Estado)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();

            var estadosFactura = await db.Facturas.AsNoTracking()
                .Select(f => f.Estado)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();

            var hayPublicaciones = await db.Publicacions.AnyAsync();
            var hayContratos = await db.Contratos.AnyAsync();

            return new FiltrosReporteDto
            {
                Ciudades = ciudades,
                Tipos = tipos,
                Operaciones = operaciones,
                Monedas = monedas,
                EstadosContrato = estadosContrato,
                EstadosFactura = estadosFactura,
                PrimeraPublicacion = hayPublicaciones
                    ? (DateOnly?)await db.Publicacions.MinAsync(p => p.FechaPublicacion)
                    : null,
                UltimaPublicacion = hayPublicaciones
                    ? (DateOnly?)await db.Publicacions.MaxAsync(p => p.FechaPublicacion)
                    : null,
                PrimerContrato = hayContratos
                    ? (DateOnly?)await db.Contratos.MinAsync(c => c.FechaInicio)
                    : null,
                UltimoContrato = hayContratos
                    ? (DateOnly?)await db.Contratos.MaxAsync(c => c.FechaInicio)
                    : null
            };
        }
    }
}
