using InmoWeb.DTOs;
using InmoWeb.Excepciones;
using InmoWeb.Services.Reportes.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InmoWeb.Controllers
{
    /// <summary>
    /// Los cinco reportes del Trabajo Practico Integrador.
    /// Cada uno se apoya en un objeto de la base de datos: un procedimiento
    /// almacenado, una vista o una funcion, y recibe por query string los
    /// filtros que el usuario carga en pantalla.
    /// </summary>
    [ApiController]
    [Route("/[controller]")]
    public class ReporteController : ControllerBase
    {
        private readonly IReporteServices service;

        public ReporteController(IReporteServices service)
        {
            this.service = service;
        }

        /// <summary>
        /// Reporte 1 - Inmuebles disponibles.
        /// Fuente: sp_BuscarInmuebles, que internamente consulta
        /// fn_InmueblesDisponibles. Filtros de correspondencia (operacion,
        /// tipo, ciudad) y de rango (precio minimo y maximo).
        /// </summary>
        [HttpGet("busqueda-inmuebles")]
        public async Task<ActionResult<ReporteBusquedaDto>> BuscarInmuebles(
            [FromQuery] string? operacion = null,
            [FromQuery] string? tipo = null,
            [FromQuery] string? ciudad = null,
            [FromQuery] decimal? precioMin = null,
            [FromQuery] decimal? precioMax = null)
        {
            try
            {
                return Ok(await service.BuscarInmueblesAsync(
                    operacion, tipo, ciudad, precioMin, precioMax));
            }
            catch (BusinesException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Reporte 2 - Publicaciones activas.
        /// Fuente: vw_PublicacionesActivas. Incluye filtro de tiempo por
        /// rango de fecha de publicacion.
        /// </summary>
        [HttpGet("publicaciones")]
        public async Task<ActionResult<ReportePublicacionesDto>> GetPublicaciones(
            [FromQuery] string? operacion = null,
            [FromQuery] string? tipo = null,
            [FromQuery] string? ciudad = null,
            [FromQuery] string? moneda = null,
            [FromQuery] decimal? precioMin = null,
            [FromQuery] decimal? precioMax = null,
            [FromQuery] DateOnly? desde = null,
            [FromQuery] DateOnly? hasta = null)
        {
            try
            {
                return Ok(await service.GetPublicacionesAsync(
                    operacion, tipo, ciudad, moneda, precioMin, precioMax, desde, hasta));
            }
            catch (BusinesException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Reporte 3 - Contratos y comisiones.
        /// Fuente: vw_ContratosDetalle, cuya columna comision_calculada sale
        /// de la funcion escalar fn_ComisionContrato.
        /// </summary>
        [HttpGet("contratos")]
        public async Task<ActionResult<ReporteContratosDto>> GetContratos(
            [FromQuery] DateOnly? desde = null,
            [FromQuery] DateOnly? hasta = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? operacion = null)
        {
            try
            {
                return Ok(await service.GetContratosAsync(desde, hasta, estado, operacion));
            }
            catch (BusinesException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Reporte 4 - Facturacion por periodo.
        /// Fuente: vw_FacturasTotales, que agrupa los detalles de cada factura.
        /// </summary>
        [HttpGet("facturacion")]
        public async Task<ActionResult<ReporteFacturacionDto>> GetFacturacion(
            [FromQuery] DateOnly? desde = null,
            [FromQuery] DateOnly? hasta = null,
            [FromQuery] string? estado = null)
        {
            try
            {
                return Ok(await service.GetFacturacionAsync(desde, hasta, estado));
            }
            catch (BusinesException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Reporte 5 - Resumen de inmuebles por ciudad y tipo.
        /// Fuente: vw_ResumenInmobiliaria, consulta agrupada con COUNT y SUM.
        /// </summary>
        [HttpGet("resumen")]
        public async Task<ActionResult<ReporteResumenDto>> GetResumen(
            [FromQuery] string? ciudad = null,
            [FromQuery] string? tipo = null)
        {
            try
            {
                return Ok(await service.GetResumenAsync(ciudad, tipo));
            }
            catch (BusinesException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Inmobiliarias con sus metricas: propiedades totales, publicadas,
        /// vendidas, alquiladas, reservadas y disponibles.
        /// </summary>
        [HttpGet("inmobiliarias")]
        public async Task<ActionResult<List<InmobiliariaResumenDto>>> GetInmobiliarias()
        {
            return Ok(await service.GetInmobiliariasAsync());
        }

        /// <summary>
        /// Opciones de los desplegables de la pantalla de reportes,
        /// tomadas de los datos realmente cargados.
        /// </summary>
        [HttpGet("filtros")]
        public async Task<ActionResult<FiltrosReporteDto>> GetFiltros()
        {
            return Ok(await service.GetFiltrosAsync());
        }
    }
}
