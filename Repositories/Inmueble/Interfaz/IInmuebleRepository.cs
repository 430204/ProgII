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

        // ---- ABM de propiedades ----
        // El alta crea propiedades Disponibles; la edicion es la que las pasa
        // a Vendido, Alquilado o Reservado.
        Task<List<InmuebleListaDto>> GetPropiedadesListadoAsync(string? texto, int? idEstado);
        Task<bool> TieneContratosAsync(int idInmueble);
        Task<bool> EliminarInmuebleAsync(int idInmueble);
        Task<CatalogosInmuebleDto> GetCatalogosAsync();

        // Resuelve el id del estado Disponible por descripcion, sin hardcodearlo
        Task<int> GetIdEstadoDisponibleAsync();

        // Al marcar una propiedad como no disponible hay que bajar sus publicaciones
        // activas, si no seguiria apareciendo en el catalogo publico
        Task<int> DarDeBajaPublicacionesActivasAsync(int idInmueble);
    }
}
