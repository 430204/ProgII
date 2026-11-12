using InmoWeb.DTOs;
using InmoWeb.Excepciones;
using InmoWeb.Models;
using InmoWeb.Services.Inmuebles.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InmoWeb.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class InmuebleController : ControllerBase
    {
        private readonly IInmuebleServices service;

        public InmuebleController(IInmuebleServices service)
        {
            this.service = service;
        }

        /// <summary>
        /// Devuelve un inmueble por id. Requiere JWT: sirve para mostrar
        /// la autenticacion funcionando desde Swagger.
        /// </summary>
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                Inmueble? inmueble = await service.GetInmuebleById(id);

                if (inmueble == null)
                {
                    return NotFound();
                }

                return Ok(inmueble);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Catalogo publico: publicaciones activas con filtros de operacion,
        /// moneda y precio maximo. Lee la vista vw_PublicacionesActivas.
        /// </summary>
        [HttpGet("publicaciones-activas")]
        public async Task<IActionResult> GetPublicacionesActivas(
        [FromQuery] string? operacion = null,
        [FromQuery] string? moneda = null,
        [FromQuery] decimal? precioMaximo = null)
        {
            try
            {
                var publicaciones = await service.GetPublicacionesActivas(
                    operacion, moneda, precioMaximo);

                return Ok(publicaciones);
            }
            catch (BusinesException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // ================= ABM de propiedades =================

        /// <summary>
        /// Listado de propiedades con filtro por texto (codigo, titulo o ciudad)
        /// y por estado. Sin filtro de estado devuelve todas.
        /// </summary>
        [HttpGet("propiedades")]
        public async Task<ActionResult<List<InmuebleListaDto>>> GetPropiedades(
            [FromQuery] string? texto = null,
            [FromQuery] int? idEstado = null)
        {
            try
            {
                return Ok(await service.GetPropiedadesAsync(texto, idEstado));
            }
            catch (BusinesException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpGet("propiedades/{id:int}")]
        public async Task<ActionResult<InmuebleResponseDto>> GetPropiedadById(int id)
        {
            var inmueble = await service.GetPropiedadByIdAsync(id);
            return inmueble is null ? NotFound() : Ok(inmueble);
        }

        /// <summary>
        /// Alta de propiedad. Siempre nace con estado Disponible.
        /// </summary>
        [HttpPost("propiedades")]
        public async Task<ActionResult<InmuebleResponseDto>> CreatePropiedad([FromBody] InmuebleCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            try
            {
                var creado = await service.CreatePropiedadAsync(dto);
                return CreatedAtAction(nameof(GetPropiedadById), new { id = creado.IdInmueble }, creado);
            }
            catch (BusinesException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Edicion de propiedad. Es la operacion que la pasa de Disponible a
        /// Vendido, Alquilado o Reservado, y baja sus publicaciones activas.
        /// </summary>
        [HttpPut("propiedades/{id:int}")]
        public async Task<ActionResult<InmuebleResponseDto>> UpdatePropiedad(int id, [FromBody] InmuebleUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            try
            {
                var actualizado = await service.UpdatePropiedadAsync(id, dto);
                return actualizado is null ? NotFound() : Ok(actualizado);
            }
            catch (BusinesException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("propiedades/{id:int}")]
        public async Task<IActionResult> DeletePropiedad(int id)
        {
            try
            {
                var eliminado = await service.DeletePropiedadAsync(id);
                return eliminado ? NoContent() : NotFound();
            }
            catch (BusinesException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Datos para los desplegables del formulario: tipos, estados,
        /// direcciones e inmobiliarias.
        /// </summary>
        [HttpGet("catalogos")]
        public async Task<ActionResult<CatalogosInmuebleDto>> GetCatalogos()
        {
            return Ok(await service.GetCatalogosAsync());
        }
    }
}
