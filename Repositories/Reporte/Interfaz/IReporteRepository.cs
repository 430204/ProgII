using InmoWeb.DTOs;

namespace InmoWeb.Repositories.Reporte.Interfaz
{
    /// <summary>
    /// Acceso a datos de los reportes. Cada metodo apoya en un objeto de la base:
    /// un procedimiento almacenado, una vista o una funcion.
    /// </summary>
    public interface IReporteRepository
    {
        // sp_BuscarInmuebles -> internamente usa fn_InmueblesDisponibles
        Task<List<BusquedaInmuebleDto>> BuscarInmueblesAsync(
            string? operacion, string? tipo, string? ciudad,
            decimal? precioMin, decimal? precioMax);

        // vw_PublicacionesActivas
        Task<List<PublicacionReporteDto>> GetPublicacionesAsync(
            string? operacion, string? tipo, string? ciudad, string? moneda,
            decimal? precioMin, decimal? precioMax,
            DateOnly? desde, DateOnly? hasta);

        // vw_ContratosDetalle -> la comision la calcula fn_ComisionContrato
        Task<List<ContratoReporteDto>> GetContratosAsync(
            DateOnly? desde, DateOnly? hasta, string? estado, string? operacion);

        // vw_FacturasTotales
        Task<List<FacturaReporteDto>> GetFacturasAsync(
            DateOnly? desde, DateOnly? hasta, string? estado);

        // vw_ResumenInmobiliaria
        Task<List<ResumenReporteDto>> GetResumenAsync(string? ciudad, string? tipo);

        // Metricas por inmobiliaria, calculadas sobre las tablas
        Task<List<InmobiliariaResumenDto>> GetInmobiliariasAsync();

        Task<FiltrosReporteDto> GetFiltrosAsync();
    }
}
