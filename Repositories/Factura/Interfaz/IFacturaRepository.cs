
using InmoWeb.DTOs;

namespace InmoWeb.Repositories.Factura.Interfaz
{
    public interface IFacturaRepository
    {
        Task<List<FacturaDTO>> GetFacturas();
    }
}
