
using Microsoft.AspNetCore.Mvc;
using InmoWeb.Services.Factura.Interfaces;

namespace InmoWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacturaController : ControllerBase
    {
        private readonly IFacturaServices services;

        public FacturaController(IFacturaServices services)
        {
            this.services = services;
        }

        [HttpGet]
        public async Task<IActionResult> GetFacturas()
        {
            var facturas = await services.GetFacturas();

            return Ok(facturas);
        }
    }
}

