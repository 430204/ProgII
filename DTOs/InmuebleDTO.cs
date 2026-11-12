using System.ComponentModel.DataAnnotations;

namespace InmoWeb.DTOs;

// Base con las propiedades y validaciones comunes a POST y PUT
public abstract class InmuebleBaseDto
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(20, ErrorMessage = "El código no puede superar los 20 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(150, ErrorMessage = "El título no puede superar los 150 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string? Descripcion { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "La superficie total debe ser mayor a 0.")]
    public decimal SuperficieTotal { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "La superficie cubierta no puede ser negativa.")]
    public decimal? SuperficieCubierta { get; set; }

    [Range(0, 100, ErrorMessage = "Los ambientes deben estar entre 0 y 100.")]
    public int? Ambientes { get; set; }

    [Range(0, 50, ErrorMessage = "Los dormitorios deben estar entre 0 y 50.")]
    public int? Dormitorios { get; set; }

    [Range(0, 50, ErrorMessage = "Los baños deben estar entre 0 y 50.")]
    public int? Banios { get; set; }

    public bool Cochera { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "IdDireccion es obligatorio.")]
    public int IdDireccion { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "IdTipoInmueble es obligatorio.")]
    public int IdTipoInmueble { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "IdEstadoInmueble es obligatorio.")]
    public int IdEstadoInmueble { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "IdInmobiliaria es obligatorio.")]
    public int IdInmobiliaria { get; set; }
}

// DTO para POST
public class InmuebleCreateDto : InmuebleBaseDto
{
}

// DTO para PUT (reemplazo completo del recurso)
public class InmuebleUpdateDto : InmuebleBaseDto
{
}

// DTO de respuesta
public class InmuebleResponseDto
{
    public int IdInmueble { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal SuperficieTotal { get; set; }
    public decimal? SuperficieCubierta { get; set; }
    public int? Ambientes { get; set; }
    public int? Dormitorios { get; set; }
    public int? Banios { get; set; }
    public bool Cochera { get; set; }
    public int IdDireccion { get; set; }
    public int IdTipoInmueble { get; set; }
    public int IdEstadoInmueble { get; set; }
    public int IdInmobiliaria { get; set; }
}

// Fila del listado de inmuebles inactivos
public class InmuebleInactivoListDto
{
    public int IdInmueble { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string TipoInmueble { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Calle { get; set; } = string.Empty;
    public int Numero { get; set; }
    public string Ciudad { get; set; } = string.Empty;
    public decimal SuperficieTotal { get; set; }
    public int? Dormitorios { get; set; }
    public int? Banios { get; set; }
}

// Opcion para los desplegables del formulario
public class OpcionDto
{
    public int Id { get; set; }
    public string Texto { get; set; } = string.Empty;
}

public class CatalogosInmuebleDto
{
    public List<OpcionDto> Tipos { get; set; } = new();
    public List<OpcionDto> EstadosInactivos { get; set; } = new();
    public List<OpcionDto> Direcciones { get; set; } = new();
    public List<OpcionDto> Inmobiliarias { get; set; } = new();
}
