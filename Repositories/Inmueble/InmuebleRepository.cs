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
            return await db.Inmuebles.Where(i => i.IdEstadoInmueble == 1).ToListAsync();

        }

        public Task<List<Models.Inmueble>> GetInmueblesDormitorios(int cantDorm)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Models.Inmueble>> GetInmueblesInactivos()
        {
            return await db.Inmuebles.Where(i => i.IdEstadoInmueble != 1).ToListAsync();
        }

        public async Task<List<VwPublicacionesActiva>> GetPublicacionesActivas(
        string? operacion,
        string? moneda,
        decimal? precioMaximo)
        {
            var consulta = db.VwPublicacionesActivas.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(operacion))
            {
                consulta = consulta.Where(p => p.Operacion == operacion);
            }

            if (!string.IsNullOrWhiteSpace(moneda))
            {
                consulta = consulta.Where(p => p.Moneda == moneda);
            }

            if (precioMaximo.HasValue)
            {
                consulta = consulta.Where(p => p.Precio <= precioMaximo.Value);
            }

            return await consulta
                .OrderBy(p => p.Moneda)
                .ThenBy(p => p.Precio)
                .ToListAsync();
        }


        public async Task<List<InmuebleListaDto>> GetPropiedadesListadoAsync(string? texto, int? idEstado)
        {
            // El listado muestra TODAS las propiedades. Si no mostrara las Disponibles,
            // una propiedad recien dada de alta desapareceria de la tabla.
            var consulta = db.Inmuebles.AsNoTracking();

            if (idEstado.HasValue)
            {
                consulta = consulta.Where(i => i.IdEstadoInmueble == idEstado.Value);
            }

            if (!string.IsNullOrWhiteSpace(texto))
            {
                var patron = "%" + texto.Trim() + "%";
                consulta = consulta.Where(i =>
                    EF.Functions.Like(i.Codigo, patron) ||
                    EF.Functions.Like(i.Titulo, patron) ||
                    EF.Functions.Like(i.IdDireccionNavigation.IdCiudadNavigation.Nombre, patron));
            }

            return await consulta
                .OrderBy(i => i.Codigo)
                .Select(i => new InmuebleListaDto
                {
                    IdInmueble = i.IdInmueble,
                    Codigo = i.Codigo,
                    Titulo = i.Titulo,
                    TipoInmueble = i.IdTipoInmuebleNavigation.Descripcion,
                    Estado = i.IdEstadoInmuebleNavigation.Descripcion,
                    Calle = i.IdDireccionNavigation.Calle,
                    Numero = i.IdDireccionNavigation.Numero,
                    Ciudad = i.IdDireccionNavigation.IdCiudadNavigation.Nombre,
                    SuperficieTotal = i.SuperficieTotal,
                    Dormitorios = i.Dormitorios,
                    Banios = i.Banios
                })
                .ToListAsync();
        }

        public async Task<bool> TieneContratosAsync(int idInmueble)
        {
            return await db.Contratos.AnyAsync(c => c.IdPublicacionNavigation.IdInmueble == idInmueble);
        }

        public async Task<bool> EliminarInmuebleAsync(int idInmueble)
        {
            // Se borran primero las filas hijas (publicaciones y propietarios) y luego el inmueble
            await using var tx = await db.Database.BeginTransactionAsync();

            await db.Publicacions.Where(p => p.IdInmueble == idInmueble).ExecuteDeleteAsync();
            await db.PropietarioInmuebles.Where(p => p.IdInmueble == idInmueble).ExecuteDeleteAsync();
            var filas = await db.Inmuebles.Where(i => i.IdInmueble == idInmueble).ExecuteDeleteAsync();

            await tx.CommitAsync();
            return filas > 0;
        }

        public async Task<CatalogosInmuebleDto> GetCatalogosAsync()
        {
            var tipos = await db.TipoInmuebles.AsNoTracking()
                .OrderBy(t => t.Descripcion)
                .Select(t => new OpcionDto { Id = t.IdTipoInmueble, Texto = t.Descripcion })
                .ToListAsync();

            var estados = await db.EstadoInmuebles.AsNoTracking()
                .OrderBy(e => e.IdEstadoInmueble)
                .Select(e => new OpcionDto { Id = e.IdEstadoInmueble, Texto = e.Descripcion })
                .ToListAsync();

            var idDisponible = await GetIdEstadoDisponibleAsync();

            var direcciones = await db.Direccions.AsNoTracking()
                .OrderBy(d => d.IdCiudadNavigation.Nombre).ThenBy(d => d.Calle)
                .Select(d => new { d.IdDireccion, d.Calle, d.Numero, Ciudad = d.IdCiudadNavigation.Nombre })
                .ToListAsync();

            var inmobiliarias = await db.Inmobiliaria.AsNoTracking()
                .OrderBy(i => i.Nombre)
                .Select(i => new OpcionDto { Id = i.IdInmobiliaria, Texto = i.Nombre })
                .ToListAsync();

            return new CatalogosInmuebleDto
            {
                Tipos = tipos,
                Estados = estados,
                EstadosInactivos = estados.Where(e => e.Id != idDisponible).ToList(),
                EstadoDisponibleId = idDisponible,
                Direcciones = direcciones
                    .Select(d => new OpcionDto
                    {
                        Id = d.IdDireccion,
                        Texto = d.Calle + " " + d.Numero + ", " + d.Ciudad
                    })
                    .ToList(),
                Inmobiliarias = inmobiliarias
            };
        }

        public async Task<int> GetIdEstadoDisponibleAsync()
        {
            var id = await db.EstadoInmuebles.AsNoTracking()
                .Where(e => e.Descripcion == "Disponible")
                .Select(e => e.IdEstadoInmueble)
                .FirstOrDefaultAsync();

            // Si por algun motivo no estuviera cargado, cae al id del script de creacion
            return id == 0 ? 1 : id;
        }

        public async Task<int> DarDeBajaPublicacionesActivasAsync(int idInmueble)
        {
            // Se usa el stored procedure sp_DarDeBajaPublicacion para no duplicar
            // en C# la regla que ya vive en la base de datos.
            var idsPublicaciones = await db.Publicacions.AsNoTracking()
                .Where(p => p.IdInmueble == idInmueble && p.Activa)
                .Select(p => p.IdPublicacion)
                .ToListAsync();

            if (idsPublicaciones.Count == 0)
            {
                return 0;
            }

            await using var tx = await db.Database.BeginTransactionAsync();

            foreach (var idPublicacion in idsPublicaciones)
            {
                await db.Procedures.sp_DarDeBajaPublicacionAsync(idPublicacion);
            }

            await tx.CommitAsync();
            return idsPublicaciones.Count;
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
