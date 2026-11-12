namespace InmoWeb.DTOs;

/// <summary>
/// Totales agrupados por moneda. Los montos de ARS y USD nunca se suman entre si:
/// se informan por separado porque sumarlos daria un numero sin sentido.
/// </summary>
public class TotalPorMonedaDto
{
    public string Moneda { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
    public decimal? Promedio { get; set; }
    public decimal? Comision { get; set; }
}

// ---------------------------------------------------------------------------
// Reporte 1 - Busqueda de inmuebles disponibles (sp_BuscarInmuebles)
// ---------------------------------------------------------------------------

public class BusquedaInmuebleDto
{
    public int IdInmueble { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string TipoInmueble { get; set; } = string.Empty;
    public string Operacion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Calle { get; set; } = string.Empty;
    public int Numero { get; set; }
    public string? Piso { get; set; }
    public string? Departamento { get; set; }
}

public class ReporteBusquedaDto
{
    public List<BusquedaInmuebleDto> Filas { get; set; } = new();
    public int Cantidad { get; set; }
    public List<TotalPorMonedaDto> Totales { get; set; } = new();
}

// ---------------------------------------------------------------------------
// Reporte 2 - Publicaciones activas (vw_PublicacionesActivas)
// ---------------------------------------------------------------------------

public class PublicacionReporteDto
{
    public int IdPublicacion { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Operacion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public DateOnly FechaPublicacion { get; set; }
    public string Calle { get; set; } = string.Empty;
    public int Numero { get; set; }
    public string Ciudad { get; set; } = string.Empty;
    public string TipoInmueble { get; set; } = string.Empty;
}

public class ReportePublicacionesDto
{
    public List<PublicacionReporteDto> Filas { get; set; } = new();
    public int Cantidad { get; set; }
    public List<TotalPorMonedaDto> Totales { get; set; } = new();
}

// ---------------------------------------------------------------------------
// Reporte 3 - Contratos y comisiones (vw_ContratosDetalle + fn_ComisionContrato)
// ---------------------------------------------------------------------------

public class ContratoReporteDto
{
    public int IdContrato { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Operacion { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Agente { get; set; } = string.Empty;
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public decimal MontoAcordado { get; set; }
    public decimal ComisionPorcentaje { get; set; }
    public decimal? ComisionCalculada { get; set; }
    public string Estado { get; set; } = string.Empty;

    // La vista no trae la moneda, se obtiene uniendo con la publicacion del contrato
    public string Moneda { get; set; } = string.Empty;
}

public class ReporteContratosDto
{
    public List<ContratoReporteDto> Filas { get; set; } = new();
    public int Cantidad { get; set; }
    public List<TotalPorMonedaDto> Totales { get; set; } = new();
}

// ---------------------------------------------------------------------------
// Reporte 4 - Facturacion por periodo (vw_FacturasTotales)
// ---------------------------------------------------------------------------

public class FacturaReporteDto
{
    public int IdFactura { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public int IdContrato { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public DateOnly FechaEmision { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal? Total { get; set; }
    public string Moneda { get; set; } = string.Empty;
}

public class ReporteFacturacionDto
{
    public List<FacturaReporteDto> Filas { get; set; } = new();
    public int Cantidad { get; set; }
    public int Pagadas { get; set; }
    public int Pendientes { get; set; }
    public List<TotalPorMonedaDto> Totales { get; set; } = new();
}

// ---------------------------------------------------------------------------
// Reporte 5 - Resumen por ciudad y tipo (vw_ResumenInmobiliaria)
// ---------------------------------------------------------------------------

public class ResumenReporteDto
{
    public string Ciudad { get; set; } = string.Empty;
    public string TipoInmueble { get; set; } = string.Empty;
    public int CantidadInmuebles { get; set; }
    public int Disponibles { get; set; }
    public int NoDisponibles { get; set; }
}

public class ReporteResumenDto
{
    public List<ResumenReporteDto> Filas { get; set; } = new();
    public int Cantidad { get; set; }
    public int TotalInmuebles { get; set; }
    public int TotalDisponibles { get; set; }
}

// ---------------------------------------------------------------------------
// Inmobiliarias con sus metricas
// ---------------------------------------------------------------------------

public class InmobiliariaResumenDto
{
    public int IdInmobiliaria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Cuit { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string Ciudad { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;

    public int TotalPropiedades { get; set; }
    public int Disponibles { get; set; }
    public int Vendidas { get; set; }
    public int Alquiladas { get; set; }
    public int Reservadas { get; set; }
    public int PublicacionesActivas { get; set; }
}

// ---------------------------------------------------------------------------
// Opciones para los desplegables de la pantalla de reportes
// ---------------------------------------------------------------------------

public class FiltrosReporteDto
{
    public List<string> Ciudades { get; set; } = new();
    public List<string> Tipos { get; set; } = new();
    public List<string> Operaciones { get; set; } = new();
    public List<string> Monedas { get; set; } = new();
    public List<string> EstadosContrato { get; set; } = new();
    public List<string> EstadosFactura { get; set; } = new();

    // Rango real de fechas cargadas, para proponer valores iniciales coherentes
    public DateOnly? PrimeraPublicacion { get; set; }
    public DateOnly? UltimaPublicacion { get; set; }
    public DateOnly? PrimerContrato { get; set; }
    public DateOnly? UltimoContrato { get; set; }
}
