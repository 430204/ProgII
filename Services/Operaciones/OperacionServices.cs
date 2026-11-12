using InmoWeb.DTOs;
using InmoWeb.Excepciones;
using InmoWeb.Repositories.Operacion.Interfaz;
using InmoWeb.Services.Operaciones.Interfaces;

namespace InmoWeb.Services.Operaciones
{
    /// <summary>
    /// Valida lo minimo del lado de la aplicacion y delega en los
    /// procedimientos almacenados, que son los que tienen la ultima palabra.
    /// Las reglas no se duplican: si el procedimiento ya las valida, aca no
    /// se vuelven a escribir.
    /// </summary>
    public class OperacionServices : IOperacionServices
    {
        private readonly IOperacionRepository repo;

        public OperacionServices(IOperacionRepository repo)
        {
            this.repo = repo;
        }

        public async Task<CatalogosOperacionDto> GetCatalogosAsync()
        {
            return await repo.GetCatalogosAsync();
        }

        public async Task<ResultadoOperacionDto> PublicarAsync(PublicarDto dto)
        {
            if (dto.Moneda != "ARS" && dto.Moneda != "USD")
                throw new BusinesException("La moneda debe ser ARS o USD.");

            return await repo.PublicarAsync(dto);
        }

        public async Task<ResultadoOperacionDto> ConcretarAsync(ConcretarDto dto)
        {
            if (dto.IdCliente == dto.IdAgente)
                throw new BusinesException(
                    "El cliente y el agente no pueden ser la misma persona.");

            if (dto.FechaInicio.HasValue && dto.FechaFin.HasValue
                && dto.FechaFin.Value < dto.FechaInicio.Value)
            {
                throw new BusinesException(
                    "La fecha de fin no puede ser anterior a la de inicio.");
            }

            return await repo.ConcretarAsync(dto);
        }

        public async Task<ResultadoOperacionDto> FacturarAsync(FacturarDto dto)
        {
            dto.NumeroFactura = dto.NumeroFactura.Trim();
            dto.Concepto = dto.Concepto.Trim();

            return await repo.FacturarAsync(dto);
        }

        public async Task<ResultadoOperacionDto> CambiarEstadoAsync(CambiarEstadoDto dto)
        {
            dto.Estado = dto.Estado.Trim();

            return await repo.CambiarEstadoAsync(dto);
        }
    }
}
