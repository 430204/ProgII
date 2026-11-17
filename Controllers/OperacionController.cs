using InmoWeb.DTOs;
using InmoWeb.Excepciones;
using InmoWeb.Services.Operaciones.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InmoWeb.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class OperacionController : ControllerBase
    {
        private readonly IOperacionServices service;

        public OperacionController(IOperacionServices service)
        {
            this.service = service;
        }

        [HttpGet("catalogos")]
        public async Task<ActionResult<CatalogosOperacionDto>> GetCatalogos()
        {
            return Ok(await service.GetCatalogosAsync());
        }

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
