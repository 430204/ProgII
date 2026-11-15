
using InmoWeb.DTOs;

namespace InmoWeb.Services.Factura.Interfaces
{
    public interface IFacturaServices
    {
        Task<List<FacturaDTO>> GetFacturas();
    }
}

