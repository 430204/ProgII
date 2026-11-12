
using Microsoft.AspNetCore.Mvc;
using InmoWeb.DTOs;
using InmoWeb.Services.DetalleFactura.Interfaces;

namespace InmoWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetalleFacturaController : ControllerBase
    {
        private readonly IDetalleFacturaServices services;

        public DetalleFacturaController(IDetalleFacturaServices services)
        {
            this.services = services;
        }

        // GET: api/DetalleFactura
        [HttpGet]
        public async Task<IActionResult> GetDetalles()
        {
            var detalles = await services.GetDetalles();

            return Ok(detalles);
        }

        // GET: api/DetalleFactura/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var detalle = await services.GetById(id);

            if (detalle == null)
                return NotFound("El detalle no existe.");

            return Ok(detalle);
        }

        // POST: api/DetalleFactura
        [HttpPost]
        public async Task<IActionResult> Agregar(
            [FromBody] DetalleFacturaDTO dto)
        {
            try
            {
                var detalle = await services.Agregar(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = detalle.IdDetalleFactura },
                    detalle);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/DetalleFactura/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Modificar(
            int id,
            [FromBody] DetalleFacturaDTO dto)
        {
            try
            {
                bool modificado = await services.Modificar(id, dto);

                if (!modificado)
                    return NotFound("El detalle no existe.");

                return Ok("Detalle modificado correctamente.");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/DetalleFactura/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            bool eliminado = await services.Eliminar(id);

            if (!eliminado)
                return NotFound("El detalle no existe.");

            return Ok("Detalle eliminado correctamente.");
        }
    }
}
