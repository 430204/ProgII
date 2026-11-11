using InmoWeb.DTOs;
using InmoWeb.Excepciones;
using InmoWeb.Models;
using InmoWeb.Services.Inmuebles.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

        [Authorize]
        [HttpGet]
        [Route("/{id}")]
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


        [HttpPost]
        [Route("/Crear Inmueble")]
        public async Task<ActionResult<InmuebleResponseDto>> Create([FromBody] InmuebleCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            try
            {
                var creado = await service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = creado.IdInmueble }, creado);
            }
            catch (BusinesException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }


        [HttpPut("{id:int}")]
        public async Task<ActionResult<InmuebleResponseDto>> Update(int id, [FromBody] InmuebleUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            try
            {
                var actualizado = await service.UpdateAsync(id, dto);
                return actualizado is null ? NotFound() : Ok(actualizado);
            }
            catch (BusinesException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

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

    }
}
