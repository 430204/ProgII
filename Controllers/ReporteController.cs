using InmoWeb.DTOs;
using InmoWeb.Excepciones;
using InmoWeb.Services.Reportes.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InmoWeb.Controllers
{

    [ApiController]
    [Route("/[controller]")]
    public class ReporteController : ControllerBase
    {
        private readonly IReporteServices service;

        public ReporteController(IReporteServices service)
        {
            this.service = service;
        }

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

        [HttpGet("inmobiliarias")]
        public async Task<ActionResult<List<InmobiliariaResumenDto>>> GetInmobiliarias()
        {
            return Ok(await service.GetInmobiliariasAsync());
        }

        [HttpGet("filtros")]
        public async Task<ActionResult<FiltrosReporteDto>> GetFiltros()
        {
            return Ok(await service.GetFiltrosAsync());
        }
    }
}
