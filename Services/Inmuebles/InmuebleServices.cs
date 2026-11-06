using InmoWeb.DTOs;
using InmoWeb.Excepciones;
using InmoWeb.Models;
using InmoWeb.Repositories.Inmueble.Interfaz;
using InmoWeb.Services.Inmuebles.Interfaces;

namespace InmoWeb.Services.Inmuebles
{
    public class InmuebleServices : IInmuebleServices
    {
        private readonly IInmuebleRepository repo;

        public InmuebleServices(IInmuebleRepository repo)
        {
            this.repo = repo;
        }

        public async Task<InmuebleResponseDto> CreateAsync(InmuebleCreateDto dto)
        {
            await ValidarReglasAsync(dto, idExcluir: null);
            var inmueble = new Inmueble();
            MapFromDto(dto, inmueble);
            await repo.AddAsync(inmueble);
            await repo.SaveChangesAsync();
            return MapToResponse(inmueble);
        }

        public async Task<Inmueble?> GetInmuebleById(int id)
        {
            return await repo.GetInmuebleById(id);
        }

        public Task<Inmueble?> GetInmuebleByTipo(int IdTipoInmueble)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Inmueble>> GetInmueblesActivos()
        {
            return await repo.GetInmueblesActivos();
        }

        public Task<List<Inmueble>> GetInmueblesDormitorios(int cantDorm)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Inmueble>> GetInmueblesInactivos()
        {
            return await repo.GetInmueblesInactivos();
        }

        public async Task<List<VwPublicacionesActiva>>? GetPublicacionesActivas()
        {
            return await repo.GetPublicacionesActivas();
        }

        public async Task<InmuebleResponseDto?> UpdateAsync(int id, InmuebleUpdateDto dto)
        {
            var inmueble = await repo.GetInmuebleById(id);
            if (inmueble is null)
                return null;
            await ValidarReglasAsync(dto, idExcluir: id);
            MapFromDto(dto, inmueble);
            repo.Update(inmueble);
            await repo.SaveChangesAsync();
            return MapToResponse(inmueble);
        }

    

    private async Task ValidarReglasAsync(InmuebleBaseDto dto, int? idExcluir)
        {
            if (dto.SuperficieCubierta.HasValue && dto.SuperficieCubierta > dto.SuperficieTotal)
                throw new BusinesException("La superficie cubierta no puede ser mayor que la superficie total.");
            if (await repo.ExisteCodigoAsync(dto.Codigo, idExcluir))
                throw new BusinesException($"Ya existe un inmueble con el código '{dto.Codigo}'.");
            var referenciasOk = await repo.ReferenciasExistenAsync(
                dto.IdDireccion, dto.IdTipoInmueble, dto.IdEstadoInmueble, dto.IdInmobiliaria);
            if (!referenciasOk)
                throw new BusinesException("Alguna de las referencias (dirección, tipo, estado o inmobiliaria) no existe.");
        }

        private static void MapFromDto(InmuebleBaseDto dto, Inmueble entity)
        {
            entity.Codigo = dto.Codigo.Trim();
            entity.Titulo = dto.Titulo.Trim();
            entity.Descripcion = dto.Descripcion?.Trim();
            entity.SuperficieTotal = dto.SuperficieTotal;
            entity.SuperficieCubierta = dto.SuperficieCubierta;
            entity.Ambientes = dto.Ambientes;
            entity.Dormitorios = dto.Dormitorios;
            entity.Banios = dto.Banios;
            entity.Cochera = dto.Cochera;
            entity.IdDireccion = dto.IdDireccion;
            entity.IdTipoInmueble = dto.IdTipoInmueble;
            entity.IdEstadoInmueble = dto.IdEstadoInmueble;
            entity.IdInmobiliaria = dto.IdInmobiliaria;
        }
        private static InmuebleResponseDto MapToResponse(Inmueble entity)
        {
            return new InmuebleResponseDto
            {
                IdInmueble = entity.IdInmueble,
                Codigo = entity.Codigo,
                Titulo = entity.Titulo,
                Descripcion = entity.Descripcion,
                SuperficieTotal = entity.SuperficieTotal,
                SuperficieCubierta = entity.SuperficieCubierta,
                Ambientes = entity.Ambientes,
                Dormitorios = entity.Dormitorios,
                Banios = entity.Banios,
                Cochera = entity.Cochera,
                IdDireccion = entity.IdDireccion,
                IdTipoInmueble = entity.IdTipoInmueble,
                IdEstadoInmueble = entity.IdEstadoInmueble,
                IdInmobiliaria = entity.IdInmobiliaria
                };
            }
        }

    }
