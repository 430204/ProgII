
using InmoWeb.DTOs;

namespace InmoWeb.Services.DetalleFactura.Interfaces
{
    public interface IDetalleFacturaServices
    {
        Task<List<DetalleFacturaRespuestaDTO>> GetDetalles();

        Task<DetalleFacturaRespuestaDTO?> GetById(int id);

        Task<DetalleFacturaRespuestaDTO> Agregar(DetalleFacturaDTO dto);

        Task<bool> Modificar(int id, DetalleFacturaDTO dto);

        Task<bool> Eliminar(int id);
    }
}
