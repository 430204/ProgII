using InmoWeb.DTOs;
using InmoWeb.Models;

namespace InmoWeb.Repositories.Inmueble.Interfaz
{
    public interface IInmuebleRepository
    {
        Task<Models.Inmueble?> GetInmuebleById(int id);

        Task<Models.Inmueble?> GetInmuebleByTipo(int IdTipoInmueble);

        Task<List<Models.Inmueble>> GetInmueblesActivos();

        Task<List<Models.Inmueble>> GetInmueblesInactivos();

        Task<List<Models.Inmueble>> GetInmueblesDormitorios(int cantDorm);

        Task<bool> ExisteCodigoAsync(string codigo, int? excluirIdInmueble = null);
        Task<bool> ReferenciasExistenAsync(int idDireccion, int idTipoInmueble, int idEstadoInmueble, int idInmobiliaria);
        Task AddAsync(Models.Inmueble inmueble);
        void Update(Models.Inmueble inmueble);

        Task<int> SaveChangesAsync();

        Task<List<VwPublicacionesActiva>> GetPublicacionesActivas(
        string? operacion,
        string? moneda,
        decimal? precioMaximo);

        // ---- CRUD de inmuebles inactivos (estado distinto de Disponible) ----
        Task<List<InmuebleInactivoListDto>> GetInactivosListadoAsync(string? texto, int? idEstado);
        Task<bool> TieneContratosAsync(int idInmueble);
        Task<bool> EliminarInmuebleAsync(int idInmueble);
        Task<CatalogosInmuebleDto> GetCatalogosAsync();
    }
}
