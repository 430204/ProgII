using InmoWeb.DTOs;

namespace InmoWeb.Services.Reportes.Interfaces
{
    public interface IReporteServices
    {
        Task<ReporteBusquedaDto> BuscarInmueblesAsync(
            string? operacion, string? tipo, string? ciudad,
            decimal? precioMin, decimal? precioMax);

        Task<ReportePublicacionesDto> GetPublicacionesAsync(
            string? operacion, string? tipo, string? ciudad, string? moneda,
            decimal? precioMin, decimal? precioMax,
            DateOnly? desde, DateOnly? hasta);

        Task<ReporteContratosDto> GetContratosAsync(
            DateOnly? desde, DateOnly? hasta, string? estado, string? operacion);

        Task<ReporteFacturacionDto> GetFacturacionAsync(
            DateOnly? desde, DateOnly? hasta, string? estado);

        Task<ReporteResumenDto> GetResumenAsync(string? ciudad, string? tipo);

        Task<List<InmobiliariaResumenDto>> GetInmobiliariasAsync();

        Task<FiltrosReporteDto> GetFiltrosAsync();
    }
}
