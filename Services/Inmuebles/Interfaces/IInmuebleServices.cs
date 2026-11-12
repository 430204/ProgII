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

        // ---- ABM de propiedades ----
        // Alta: la propiedad nace Disponible.
        // Edicion: es la operacion que la pasa a Vendido, Alquilado o Reservado.
        Task<List<InmuebleListaDto>> GetPropiedadesAsync(string? texto, int? idEstado);
        Task<InmuebleResponseDto?> GetPropiedadByIdAsync(int id);
        Task<InmuebleResponseDto> CreatePropiedadAsync(InmuebleCreateDto dto);
        Task<InmuebleResponseDto?> UpdatePropiedadAsync(int id, InmuebleUpdateDto dto);
        Task<bool> DeletePropiedadAsync(int id);
        Task<CatalogosInmuebleDto> GetCatalogosAsync();
    }
}
