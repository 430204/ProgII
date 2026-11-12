
using InmoWeb.DTOs;
using InmoWeb.Repositories.Factura.Interfaz;
using InmoWeb.Services.Factura.Interfaces;

namespace InmoWeb.Services.Factura
{
    public class FacturaServices : IFacturaServices
    {
        private readonly IFacturaRepository repository;

        public FacturaServices(IFacturaRepository repository)
        {
            this.repository = repository;
        }

        public async Task<List<FacturaDTO>> GetFacturas()
        {
            return await repository.GetFacturas();
        }
    }
}

