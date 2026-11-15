using System.ComponentModel.DataAnnotations;

namespace InmoWeb.DTOs;

// ---------------------------------------------------------------------------
// Opciones para los desplegables de la pantalla de operaciones
// ---------------------------------------------------------------------------

public class PublicacionOpcionDto
{
    public int IdPublicacion { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Operacion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
}

public class ContratoOpcionDto
{
    public int IdContrato { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public decimal MontoAcordado { get; set; }
    public decimal ComisionPorcentaje { get; set; }
    public decimal ComisionSugerida { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public int CantidadFacturas { get; set; }
}

public class CatalogosOperacionDto
{
    // Inmuebles disponibles que todavia no tienen publicacion activa
    public List<OpcionDto> Publicables { get; set; } = new();

    // Publicaciones activas: son las que se pueden concretar
    public List<PublicacionOpcionDto> Publicaciones { get; set; } = new();

    public List<ContratoOpcionDto> Contratos { get; set; } = new();
    public List<OpcionDto> Clientes { get; set; } = new();
    public List<OpcionDto> Agentes { get; set; } = new();
    public List<OpcionDto> Operaciones { get; set; } = new();
    public List<OpcionDto> Inmuebles { get; set; } = new();
    public List<string> Estados { get; set; } = new();
    public List<string> Monedas { get; set; } = new();
}

// ---------------------------------------------------------------------------
// Entradas de cada operacion
// ---------------------------------------------------------------------------

public class PublicarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Elegí un inmueble.")]
    public int IdInmueble { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Elegí el tipo de operación.")]
    public int IdTipoOperacion { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero.")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "La moneda es obligatoria.")]
    public string Moneda { get; set; } = "ARS";
}

public class ConcretarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Elegí una publicación.")]
    public int IdPublicacion { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Elegí el cliente.")]
    public int IdCliente { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Elegí el agente.")]
    public int IdAgente { get; set; }

    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que cero.")]
    public decimal Monto { get; set; }

    [Range(0, 100, ErrorMessage = "La comisión debe estar entre 0 y 100.")]
    public decimal Comision { get; set; }
}

public class FacturarDto
{
    [Required(ErrorMessage = "El número de factura es obligatorio.")]
    [StringLength(30, ErrorMessage = "El número no puede superar los 30 caracteres.")]
    public string NumeroFactura { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Elegí el contrato.")]
    public int IdContrato { get; set; }

    public DateOnly? FechaVencimiento { get; set; }

    [Required(ErrorMessage = "El concepto es obligatorio.")]
    [StringLength(200, ErrorMessage = "El concepto no puede superar los 200 caracteres.")]
    public string Concepto { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
    public int Cantidad { get; set; } = 1;

    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario no puede ser negativo.")]
    public decimal PrecioUnitario { get; set; }
}

public class CambiarEstadoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Elegí un inmueble.")]
    public int IdInmueble { get; set; }

    [Required(ErrorMessage = "Elegí el estado.")]
    public string Estado { get; set; } = string.Empty;
}

// ---------------------------------------------------------------------------
// Respuesta: ademas de confirmar, informa que cambio en la base.
// Sirve para mostrar en pantalla el efecto de los triggers.
// ---------------------------------------------------------------------------

public class ResultadoOperacionDto
{
    public bool Exito { get; set; } = true;
    public string Mensaje { get; set; } = string.Empty;

    public int? IdGenerado { get; set; }
    public string? Codigo { get; set; }
    public string? Titulo { get; set; }

    public string? EstadoAnterior { get; set; }
    public string? EstadoNuevo { get; set; }

    public bool? SeguiaPublicado { get; set; }

    // Lineas que explican que hizo la base de datos por su cuenta
    public List<string> EfectosEnLaBase { get; set; } = new();
}
