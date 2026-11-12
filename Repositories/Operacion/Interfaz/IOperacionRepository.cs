using InmoWeb.DTOs;

namespace InmoWeb.Repositories.Operacion.Interfaz
{
    /// <summary>
    /// Operaciones del negocio resueltas con los procedimientos almacenados de
    /// la base. La logica vive en la base y no se reescribe en C#.
    /// </summary>
    public interface IOperacionRepository
    {
        Task<CatalogosOperacionDto> GetCatalogosAsync();

        // sp_CrearPublicacion
        Task<ResultadoOperacionDto> PublicarAsync(PublicarDto dto);

        // sp_CrearContrato -> dispara tr_Contrato_SinSolapamiento
        //                    y tr_Contrato_ActualizarDisponibilidad
        Task<ResultadoOperacionDto> ConcretarAsync(ConcretarDto dto);

        // sp_EmitirFactura -> dispara tr_DetalleFactura_CalcularSubtotal
        Task<ResultadoOperacionDto> FacturarAsync(FacturarDto dto);

        // sp_CambiarEstadoInmueble
        Task<ResultadoOperacionDto> CambiarEstadoAsync(CambiarEstadoDto dto);
    }
}
