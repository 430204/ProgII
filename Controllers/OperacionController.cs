using InmoWeb.DTOs;
using InmoWeb.Excepciones;
using InmoWeb.Services.Operaciones.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InmoWeb.Controllers
{
    /// <summary>
    /// Operaciones del negocio. Cada endpoint ejecuta un procedimiento
    /// almacenado y devuelve, ademas del resultado, que hizo la base por
    /// su cuenta a traves de los triggers.
    /// </summary>
    [ApiController]
    [Route("/[controller]")]
    public class OperacionController : ControllerBase
    {
        private readonly IOperacionServices service;

        public OperacionController(IOperacionServices service)
        {
            this.service = service;
        }

        /// <summary>
        /// Datos para los desplegables: inmuebles publicables, publicaciones
        /// activas, contratos, clientes, agentes y estados.
        /// </summary>
        [HttpGet("catalogos")]
        public async Task<ActionResult<CatalogosOperacionDto>> GetCatalogos()
        {
            return Ok(await service.GetCatalogosAsync());
        }

        /// <summary>Publica una propiedad disponible. Usa sp_CrearPublicacion.</summary>
        [HttpPost("publicar")]
        public async Task<ActionResult<ResultadoOperacionDto>> Publicar(
            [FromBody] PublicarDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                return Ok(await service.PublicarAsync(dto));
            }
            catch (BusinesException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Concreta una venta o un alquiler. Usa sp_CrearContrato; los triggers
        /// cierran la publicacion y cambian el estado del inmueble.
        /// </summary>
        [HttpPost("concretar")]
        public async Task<ActionResult<ResultadoOperacionDto>> Concretar(
            [FromBody] ConcretarDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                return Ok(await service.ConcretarAsync(dto));
            }
            catch (BusinesException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        /// <summary>Emite una factura de un contrato. Usa sp_EmitirFactura.</summary>
        [HttpPost("facturar")]
        public async Task<ActionResult<ResultadoOperacionDto>> Facturar(
            [FromBody] FacturarDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                return Ok(await service.FacturarAsync(dto));
            }
            catch (BusinesException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        /// <summary>Cambia el estado de un inmueble. Usa sp_CambiarEstadoInmueble.</summary>
        [HttpPost("cambiar-estado")]
        public async Task<ActionResult<ResultadoOperacionDto>> CambiarEstado(
            [FromBody] CambiarEstadoDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                return Ok(await service.CambiarEstadoAsync(dto));
            }
            catch (BusinesException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }
    }
}
