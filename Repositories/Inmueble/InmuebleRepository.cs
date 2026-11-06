using InmoWeb.Repositories.Inmueble.Interfaz;
using InmoWeb.Models;
using Microsoft.EntityFrameworkCore;
using InmoWeb.DTOs;

namespace InmoWeb.Repositories.Inmueble
{
    public class InmuebleRepository : IInmuebleRepository
    {
        private readonly InmobiliariaDBContext db;

        public InmuebleRepository(InmobiliariaDBContext db)
        {
            this.db = db;
        }

        public async Task AddAsync(Models.Inmueble inmueble)
        {
             await db.Inmuebles.AddAsync(inmueble);
        }

        public Task<bool> CrearInmueble()
        {
            throw new NotImplementedException();
        }

        public Task<bool> EliminarInmueble()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ExisteCodigoAsync(string codigo, int? excluirIdInmueble = null)
        {
            return await db.Inmuebles.AnyAsync(i =>
            i.Codigo == codigo &&
            (excluirIdInmueble == null || i.IdInmueble != excluirIdInmueble));
        }

        public Task<Models.Inmueble>? GetInmuebleById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Models.Inmueble?> GetInmuebleByTipo(int IdTipoInmueble)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Models.Inmueble>> GetInmueblesActivos()
        {
            return await db.Inmuebles.Include(i => i.IdEstadoInmueble).Where(i => i.IdEstadoInmueble == 1).ToListAsync();

        }

        public Task<List<Models.Inmueble>> GetInmueblesDormitorios(int cantDorm)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Models.Inmueble>> GetInmueblesInactivos()
        {
            return await db.Inmuebles.Include(i => i.IdEstadoInmueble).Where(i => i.IdEstadoInmueble == 2).ToListAsync();
        }

        public async Task<List<VwPublicacionesActiva>> GetPublicacionesActivas()
        {
            return await db.VwPublicacionesActivas
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<bool> ReferenciasExistenAsync(int idDireccion, int idTipoInmueble, int idEstadoInmueble, int idInmobiliaria)
        {
            var direccionOk = await db.Set<Direccion>().AnyAsync(d => d.IdDireccion == idDireccion);
            var tipoOk = await db.Set<TipoInmueble>().AnyAsync(t => t.IdTipoInmueble == idTipoInmueble);
            var estadoOk = await db.Set<EstadoInmueble>().AnyAsync(e => e.IdEstadoInmueble == idEstadoInmueble);
            var inmobiliariaOk = await db.Set<Inmobiliarium>().AnyAsync(i => i.IdInmobiliaria == idInmobiliaria);
            return direccionOk && tipoOk && estadoOk && inmobiliariaOk;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await db.SaveChangesAsync();
        }

        public void Update(Models.Inmueble inmueble)
        {
            db.Inmuebles.Update(inmueble);
        }

        async Task<Models.Inmueble?> IInmuebleRepository.GetInmuebleById(int id)
        {
            return await db.Inmuebles.FindAsync(id);
        }
    }
}
