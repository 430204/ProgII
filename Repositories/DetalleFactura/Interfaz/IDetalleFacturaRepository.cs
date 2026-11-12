
using InmoWeb.Models;

namespace InmoWeb.Repositories.DetalleFactura.Interfaz
{
    public interface IDetalleFacturaRepository
    {
        Task<List<Models.DetalleFactura>> GetDetalles();

        Task<Models.DetalleFactura?> GetById(int id);

        Task<bool> ExisteFactura(int idFactura);

        Task Agregar(Models.DetalleFactura detalle);

        void Modificar(Models.DetalleFactura detalle);

        void Eliminar(Models.DetalleFactura detalle);

        Task GuardarCambios();
    }
}
