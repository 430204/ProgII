using InmoWeb.DTOs;
using InmoWeb.Excepciones;
using InmoWeb.Repositories.Reporte.Interfaz;
using InmoWeb.Services.Reportes.Interfaces;

namespace InmoWeb.Services.Reportes
{
    /// <summary>
    /// Reglas de los reportes: valida los filtros que escribe el usuario y
    /// arma los totales. Los montos se agrupan por moneda y nunca se suman
    /// entre ARS y USD.
    /// </summary>
    public class ReporteServices : IReporteServices
    {
        private readonly IReporteRepository repo;

        public ReporteServices(IReporteRepository repo)
        {
            this.repo = repo;
        }

        // ------------------------------------------------------------------
        // Reporte 1 - Busqueda de disponibles (sp_BuscarInmuebles)
        // ------------------------------------------------------------------
        public async Task<ReporteBusquedaDto> BuscarInmueblesAsync(
            string? operacion, string? tipo, string? ciudad,
            decimal? precioMin, decimal? precioMax)
        {
            ValidarRangoDePrecios(precioMin, precioMax);

            var filas = await repo.BuscarInmueblesAsync(
                Limpiar(operacion), Limpiar(tipo), Limpiar(ciudad), precioMin, precioMax);

            return new ReporteBusquedaDto
            {
                Filas = filas,
                Cantidad = filas.Count,
                Totales = filas
                    .GroupBy(f => f.Moneda)
                    .Select(g => new TotalPorMonedaDto
                    {
                        Moneda = g.Key,
                        Cantidad = g.Count(),
                        Monto = g.Sum(f => f.Precio),
                        Promedio = Math.Round(g.Average(f => f.Precio), 2)
                    })
                    .OrderBy(t => t.Moneda)
                    .ToList()
            };
        }

        // ------------------------------------------------------------------
        // Reporte 2 - Publicaciones activas (vw_PublicacionesActivas)
        // ------------------------------------------------------------------
        public async Task<ReportePublicacionesDto> GetPublicacionesAsync(
            string? operacion, string? tipo, string? ciudad, string? moneda,
            decimal? precioMin, decimal? precioMax,
            DateOnly? desde, DateOnly? hasta)
        {
            ValidarRangoDePrecios(precioMin, precioMax);
            ValidarRangoDeFechas(desde, hasta);

            if (!string.IsNullOrWhiteSpace(moneda) && moneda != "ARS" && moneda != "USD")
                throw new BusinesException("La moneda debe ser ARS o USD.");

            var filas = await repo.GetPublicacionesAsync(
                Limpiar(operacion), Limpiar(tipo), Limpiar(ciudad), Limpiar(moneda),
                precioMin, precioMax, desde, hasta);

            return new ReportePublicacionesDto
            {
                Filas = filas,
                Cantidad = filas.Count,
                Totales = filas
                    .GroupBy(f => f.Moneda)
                    .Select(g => new TotalPorMonedaDto
                    {
                        Moneda = g.Key,
                        Cantidad = g.Count(),
                        Monto = g.Sum(f => f.Precio),
                        Promedio = Math.Round(g.Average(f => f.Precio), 2)
                    })
                    .OrderBy(t => t.Moneda)
                    .ToList()
            };
        }

        // ------------------------------------------------------------------
        // Reporte 3 - Contratos y comisiones (vw_ContratosDetalle)
        // ------------------------------------------------------------------
        public async Task<ReporteContratosDto> GetContratosAsync(
            DateOnly? desde, DateOnly? hasta, string? estado, string? operacion)
        {
            ValidarRangoDeFechas(desde, hasta);

            var filas = await repo.GetContratosAsync(
                desde, hasta, Limpiar(estado), Limpiar(operacion));

            return new ReporteContratosDto
            {
                Filas = filas,
                Cantidad = filas.Count,
                Totales = filas
                    .GroupBy(f => f.Moneda)
                    .Select(g => new TotalPorMonedaDto
                    {
                        Moneda = g.Key,
                        Cantidad = g.Count(),
                        Monto = g.Sum(f => f.MontoAcordado),
                        Comision = g.Sum(f => f.ComisionCalculada ?? 0m)
                    })
                    .OrderBy(t => t.Moneda)
                    .ToList()
            };
        }

        // ------------------------------------------------------------------
        // Reporte 4 - Facturacion por periodo (vw_FacturasTotales)
        // ------------------------------------------------------------------
        public async Task<ReporteFacturacionDto> GetFacturacionAsync(
            DateOnly? desde, DateOnly? hasta, string? estado)
        {
            ValidarRangoDeFechas(desde, hasta);

            var filas = await repo.GetFacturasAsync(desde, hasta, Limpiar(estado));

            return new ReporteFacturacionDto
            {
                Filas = filas,
                Cantidad = filas.Count,
                Pagadas = filas.Count(f => f.Estado == "Pagada"),
                Pendientes = filas.Count(f => f.Estado == "Pendiente"),
                Totales = filas
                    .GroupBy(f => f.Moneda)
                    .Select(g => new TotalPorMonedaDto
                    {
                        Moneda = g.Key,
                        Cantidad = g.Count(),
                        Monto = g.Sum(f => f.Total ?? 0m),
                        Promedio = g.Any()
                            ? Math.Round(g.Average(f => f.Total ?? 0m), 2)
                            : 0m
                    })
                    .OrderBy(t => t.Moneda)
                    .ToList()
            };
        }

        // ------------------------------------------------------------------
        // Reporte 5 - Resumen por ciudad y tipo (vw_ResumenInmobiliaria)
        // ------------------------------------------------------------------
        public async Task<ReporteResumenDto> GetResumenAsync(string? ciudad, string? tipo)
        {
            var filas = await repo.GetResumenAsync(Limpiar(ciudad), Limpiar(tipo));

            return new ReporteResumenDto
            {
                Filas = filas,
                Cantidad = filas.Count,
                TotalInmuebles = filas.Sum(f => f.CantidadInmuebles),
                TotalDisponibles = filas.Sum(f => f.Disponibles)
            };
        }

        public async Task<List<InmobiliariaResumenDto>> GetInmobiliariasAsync()
        {
            return await repo.GetInmobiliariasAsync();
        }

        public async Task<FiltrosReporteDto> GetFiltrosAsync()
        {
            return await repo.GetFiltrosAsync();
        }

        // ------------------------------------------------------------------
        // Validaciones comunes
        // ------------------------------------------------------------------
        private static void ValidarRangoDePrecios(decimal? minimo, decimal? maximo)
        {
            if (minimo.HasValue && minimo.Value < 0)
                throw new BusinesException("El precio mínimo no puede ser negativo.");

            if (maximo.HasValue && maximo.Value <= 0)
                throw new BusinesException("El precio máximo debe ser mayor que cero.");

            if (minimo.HasValue && maximo.HasValue && minimo.Value > maximo.Value)
                throw new BusinesException(
                    "El precio mínimo no puede ser mayor que el precio máximo.");
        }

        private static void ValidarRangoDeFechas(DateOnly? desde, DateOnly? hasta)
        {
            if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
                throw new BusinesException(
                    "La fecha desde no puede ser posterior a la fecha hasta.");
        }

        private static string? Limpiar(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
