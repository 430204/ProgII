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

        Task<List<VwPublicacionesActiva>>? GetPublicacionesActivas();

        
    }
}
