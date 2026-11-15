using InmoWeb.DTOs;

namespace InmoWeb.Services.Operaciones.Interfaces
{
    public interface IOperacionServices
    {
        Task<CatalogosOperacionDto> GetCatalogosAsync();
        Task<ResultadoOperacionDto> PublicarAsync(PublicarDto dto);
        Task<ResultadoOperacionDto> ConcretarAsync(ConcretarDto dto);
        Task<ResultadoOperacionDto> FacturarAsync(FacturarDto dto);
        Task<ResultadoOperacionDto> CambiarEstadoAsync(CambiarEstadoDto dto);
    }
}
