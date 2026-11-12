using InmoWeb.DTOs;
using InmoWeb.Models;

namespace InmoWeb.Services.Inmuebles.Interfaces
{
    public interface IInmuebleServices
    {
        Task<InmoWeb.Models.Inmueble?> GetInmuebleById(int id);

        Task<InmoWeb.Models.Inmueble?> GetInmuebleByTipo(int IdTipoInmueble);

        Task<List<InmoWeb.Models.Inmueble>> GetInmueblesActivos();

        Task<List<InmoWeb.Models.Inmueble>> GetInmueblesInactivos();

        Task<List<InmoWeb.Models.Inmueble>> GetInmueblesDormitorios(int cantDorm);

        Task<InmuebleResponseDto> CreateAsync(InmuebleCreateDto dto);
        Task<InmuebleResponseDto?> UpdateAsync(int id, InmuebleUpdateDto dto);

        Task<List<VwPublicacionesActiva>> GetPublicacionesActivas(
        string? operacion,
        string? moneda,
        decimal? precioMaximo);

        // ---- CRUD de inmuebles inactivos ----
        Task<List<InmuebleInactivoListDto>> GetInactivosAsync(string? texto, int? idEstado);
        Task<InmuebleResponseDto?> GetInactivoByIdAsync(int id);
        Task<InmuebleResponseDto> CreateInactivoAsync(InmuebleCreateDto dto);
        Task<InmuebleResponseDto?> UpdateInactivoAsync(int id, InmuebleUpdateDto dto);
        Task<bool> DeleteInactivoAsync(int id);
        Task<CatalogosInmuebleDto> GetCatalogosAsync();
    }
}
