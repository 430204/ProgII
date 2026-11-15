using InmoWeb.DTOs;
using InmoWeb.Excepciones;
using InmoWeb.Models;
using InmoWeb.Repositories.Operacion.Interfaz;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InmoWeb.Repositories.Operacion
{
    public class OperacionRepository : IOperacionRepository
    {
        private readonly InmobiliariaDBContext db;

        public OperacionRepository(InmobiliariaDBContext db)
        {
            this.db = db;
        }

        // ------------------------------------------------------------------
        // Catalogos de la pantalla
        // ------------------------------------------------------------------
        public async Task<CatalogosOperacionDto> GetCatalogosAsync()
        {
            // Solo se pueden publicar inmuebles Disponibles que no tengan
            // ya una publicacion activa: es la misma regla que valida el SP.
            var publicables = await db.Inmuebles.AsNoTracking()
                .Where(i => i.IdEstadoInmuebleNavigation.Descripcion == "Disponible"
                         && !i.Publicacions.Any(p => p.Activa))
                .OrderBy(i => i.Codigo)
                .Select(i => new OpcionDto
                {
                    Id = i.IdInmueble,
                    Texto = i.Codigo + " - " + i.Titulo
                })
                .ToListAsync();

            var publicaciones = await db.Publicacions.AsNoTracking()
                .Where(p => p.Activa)
                .OrderBy(p => p.IdInmuebleNavigation.Codigo)
                .Select(p => new PublicacionOpcionDto
                {
                    IdPublicacion = p.IdPublicacion,
                    Codigo = p.IdInmuebleNavigation.Codigo,
                    Titulo = p.IdInmuebleNavigation.Titulo,
                    Operacion = p.IdTipoOperacionNavigation.Descripcion,
                    Precio = p.Precio,
                    Moneda = p.Moneda,
                    Ciudad = p.IdInmuebleNavigation.IdDireccionNavigation
                                .IdCiudadNavigation.Nombre
                })
                .ToListAsync();

            var contratos = await db.Contratos.AsNoTracking()
                .OrderByDescending(c => c.IdContrato)
                .Select(c => new ContratoOpcionDto
                {
                    IdContrato = c.IdContrato,
                    Codigo = c.IdPublicacionNavigation.IdInmuebleNavigation.Codigo,
                    Titulo = c.IdPublicacionNavigation.IdInmuebleNavigation.Titulo,
                    Cliente = c.IdClienteNavigation.Nombre + " " +
                              c.IdClienteNavigation.Apellido,
                    MontoAcordado = c.MontoAcordado,
                    ComisionPorcentaje = c.ComisionPorcentaje,
                    ComisionSugerida = Math.Round(
                        c.MontoAcordado * c.ComisionPorcentaje / 100m, 2),
                    Moneda = c.IdPublicacionNavigation.Moneda,
                    CantidadFacturas = c.Facturas.Count()
                })
                .ToListAsync();

            var clientes = await db.Personas.AsNoTracking()
                .Where(p => p.IdTipoPersonas.Any(t => t.Descripcion == "Cliente"))
                .OrderBy(p => p.Apellido).ThenBy(p => p.Nombre)
                .Select(p => new OpcionDto
                {
                    Id = p.IdPersona,
                    Texto = p.Apellido + ", " + p.Nombre + " (DNI " + p.Dni + ")"
                })
                .ToListAsync();

            var agentes = await db.Personas.AsNoTracking()
                .Where(p => p.IdTipoPersonas.Any(t => t.Descripcion == "Agente inmobiliario"))
                .OrderBy(p => p.Apellido).ThenBy(p => p.Nombre)
                .Select(p => new OpcionDto
                {
                    Id = p.IdPersona,
                    Texto = p.Apellido + ", " + p.Nombre
                })
                .ToListAsync();

            var operaciones = await db.TipoOperacions.AsNoTracking()
                .OrderBy(o => o.Descripcion)
                .Select(o => new OpcionDto
                {
                    Id = o.IdTipoOperacion,
                    Texto = o.Descripcion
                })
                .ToListAsync();

            var inmuebles = await db.Inmuebles.AsNoTracking()
                .OrderBy(i => i.Codigo)
                .Select(i => new OpcionDto
                {
                    Id = i.IdInmueble,
                    Texto = i.Codigo + " - " + i.Titulo + " (" +
                            i.IdEstadoInmuebleNavigation.Descripcion + ")"
                })
                .ToListAsync();

            var estados = await db.EstadoInmuebles.AsNoTracking()
                .OrderBy(e => e.IdEstadoInmueble)
                .Select(e => e.Descripcion)
                .ToListAsync();

            return new CatalogosOperacionDto
            {
                Publicables = publicables,
                Publicaciones = publicaciones,
                Contratos = contratos,
                Clientes = clientes,
                Agentes = agentes,
                Operaciones = operaciones,
                Inmuebles = inmuebles,
                Estados = estados,
                Monedas = new List<string> { "ARS", "USD" }
            };
        }

        // ------------------------------------------------------------------
        // sp_CrearPublicacion
        // ------------------------------------------------------------------
        public async Task<ResultadoOperacionDto> PublicarAsync(PublicarDto dto)
        {
            var inmueble = await DatosDelInmuebleAsync(dto.IdInmueble)
                ?? throw new BusinesException("El inmueble no existe.");

            await EjecutarAsync(() => db.Procedures.sp_CrearPublicacionAsync(
                dto.IdInmueble, dto.IdTipoOperacion, dto.Precio, dto.Moneda));

            var publicacion = await db.Publicacions.AsNoTracking()
                .Where(p => p.IdInmueble == dto.IdInmueble && p.Activa)
                .OrderByDescending(p => p.IdPublicacion)
                .FirstOrDefaultAsync();

            return new ResultadoOperacionDto
            {
                Mensaje = "Publicación creada para " + inmueble.Codigo + ".",
                IdGenerado = publicacion?.IdPublicacion,
                Codigo = inmueble.Codigo,
                Titulo = inmueble.Titulo,
                EstadoNuevo = inmueble.Estado,
                SeguiaPublicado = true,
                EfectosEnLaBase =
                {
                    "sp_CrearPublicacion validó el precio, la moneda y que no " +
                    "hubiera otra publicación activa para esa operación.",
                    "La propiedad ya aparece en el catálogo público."
                }
            };
        }

        // ------------------------------------------------------------------
        // sp_CrearContrato
        // Los triggers hacen el resto: cierran la publicacion y cambian el
        // estado del inmueble a Vendido o Alquilado.
        // ------------------------------------------------------------------
        public async Task<ResultadoOperacionDto> ConcretarAsync(ConcretarDto dto)
        {
            var publicacion = await db.Publicacions.AsNoTracking()
                .Where(p => p.IdPublicacion == dto.IdPublicacion)
                .Select(p => new
                {
                    p.IdInmueble,
                    p.Moneda,
                    Operacion = p.IdTipoOperacionNavigation.Descripcion,
                    Codigo = p.IdInmuebleNavigation.Codigo,
                    Titulo = p.IdInmuebleNavigation.Titulo,
                    Estado = p.IdInmuebleNavigation.IdEstadoInmuebleNavigation.Descripcion
                })
                .FirstOrDefaultAsync()
                ?? throw new BusinesException("La publicación no existe.");

            var inicio = dto.FechaInicio ?? DateOnly.FromDateTime(DateTime.Today);

            await EjecutarAsync(() => db.Procedures.sp_CrearContratoAsync(
                dto.IdPublicacion, dto.IdCliente, dto.IdAgente,
                inicio, dto.FechaFin, dto.Monto, dto.Comision));

            var contrato = await db.Contratos.AsNoTracking()
                .Where(c => c.IdPublicacion == dto.IdPublicacion)
                .OrderByDescending(c => c.IdContrato)
                .FirstOrDefaultAsync();

            var estadoNuevo = await db.Inmuebles.AsNoTracking()
                .Where(i => i.IdInmueble == publicacion.IdInmueble)
                .Select(i => i.IdEstadoInmuebleNavigation.Descripcion)
                .FirstOrDefaultAsync();

            var sigueActiva = await db.Publicacions.AsNoTracking()
                .AnyAsync(p => p.IdPublicacion == dto.IdPublicacion && p.Activa);

            var comision = Math.Round(dto.Monto * dto.Comision / 100m, 2);

            var resultado = new ResultadoOperacionDto
            {
                Mensaje = "Operación de " + publicacion.Operacion.ToLower() +
                          " concretada sobre " + publicacion.Codigo + ".",
                IdGenerado = contrato?.IdContrato,
                Codigo = publicacion.Codigo,
                Titulo = publicacion.Titulo,
                EstadoAnterior = publicacion.Estado,
                EstadoNuevo = estadoNuevo,
                SeguiaPublicado = sigueActiva
            };

            resultado.EfectosEnLaBase.Add(
                "sp_CrearContrato registró el contrato por " +
                publicacion.Moneda + " " + dto.Monto.ToString("N2") +
                " con una comisión del " + dto.Comision.ToString("N2") +
                " % (" + publicacion.Moneda + " " + comision.ToString("N2") + ").");

            if (!string.IsNullOrEmpty(estadoNuevo) && estadoNuevo != publicacion.Estado)
            {
                resultado.EfectosEnLaBase.Add(
                    "El trigger tr_Contrato_ActualizarDisponibilidad cambió el estado " +
                    "del inmueble de " + publicacion.Estado + " a " + estadoNuevo + ".");
            }

            if (!sigueActiva)
            {
                resultado.EfectosEnLaBase.Add(
                    "El mismo trigger dio de baja la publicación, así que la " +
                    "propiedad ya no figura en el catálogo público.");
            }

            resultado.EfectosEnLaBase.Add(
                "Las métricas de la inmobiliaria y los reportes 3 y 5 ya reflejan el cambio.");

            return resultado;
        }

        // ------------------------------------------------------------------
        // sp_EmitirFactura
        // ------------------------------------------------------------------
        public async Task<ResultadoOperacionDto> FacturarAsync(FacturarDto dto)
        {
            var contrato = await db.Contratos.AsNoTracking()
                .Where(c => c.IdContrato == dto.IdContrato)
                .Select(c => new
                {
                    Codigo = c.IdPublicacionNavigation.IdInmuebleNavigation.Codigo,
                    Titulo = c.IdPublicacionNavigation.IdInmuebleNavigation.Titulo,
                    c.IdPublicacionNavigation.Moneda
                })
                .FirstOrDefaultAsync()
                ?? throw new BusinesException("El contrato no existe.");

            await EjecutarAsync(() => db.Procedures.sp_EmitirFacturaAsync(
                dto.NumeroFactura, dto.IdContrato, dto.FechaVencimiento,
                dto.Concepto, dto.Cantidad, dto.PrecioUnitario));

            var factura = await db.Facturas.AsNoTracking()
                .Where(f => f.NumeroFactura == dto.NumeroFactura)
                .FirstOrDefaultAsync();

            var total = dto.Cantidad * dto.PrecioUnitario;

            return new ResultadoOperacionDto
            {
                Mensaje = "Factura " + dto.NumeroFactura + " emitida por " +
                          contrato.Moneda + " " + total.ToString("N2") + ".",
                IdGenerado = factura?.IdFactura,
                Codigo = contrato.Codigo,
                Titulo = contrato.Titulo,
                EfectosEnLaBase =
                {
                    "sp_EmitirFactura insertó la factura y su detalle dentro de una " +
                    "transacción: si algo falla, no queda una factura sin detalle.",
                    "El trigger tr_DetalleFactura_CalcularSubtotal recalculó el subtotal " +
                    "a partir de la cantidad y el precio unitario.",
                    "El reporte 4 de facturación ya la incluye."
                }
            };
        }

        // ------------------------------------------------------------------
        // sp_CambiarEstadoInmueble
        // ------------------------------------------------------------------
        public async Task<ResultadoOperacionDto> CambiarEstadoAsync(CambiarEstadoDto dto)
        {
            var inmueble = await DatosDelInmuebleAsync(dto.IdInmueble)
                ?? throw new BusinesException("El inmueble no existe.");

            await EjecutarAsync(() => db.Procedures.sp_CambiarEstadoInmuebleAsync(
                dto.IdInmueble, dto.Estado));

            var estadoNuevo = await db.Inmuebles.AsNoTracking()
                .Where(i => i.IdInmueble == dto.IdInmueble)
                .Select(i => i.IdEstadoInmuebleNavigation.Descripcion)
                .FirstOrDefaultAsync();

            return new ResultadoOperacionDto
            {
                Mensaje = "El inmueble " + inmueble.Codigo + " pasó a " + estadoNuevo + ".",
                Codigo = inmueble.Codigo,
                Titulo = inmueble.Titulo,
                EstadoAnterior = inmueble.Estado,
                EstadoNuevo = estadoNuevo,
                EfectosEnLaBase =
                {
                    "sp_CambiarEstadoInmueble resolvió el estado por su descripción " +
                    "y rechaza cualquier valor que no exista en EstadoInmueble."
                }
            };
        }

        // ------------------------------------------------------------------
        // Auxiliares
        // ------------------------------------------------------------------
        private async Task<DatosInmueble?> DatosDelInmuebleAsync(int idInmueble)
        {
            return await db.Inmuebles.AsNoTracking()
                .Where(i => i.IdInmueble == idInmueble)
                .Select(i => new DatosInmueble(
                    i.Codigo,
                    i.Titulo,
                    i.IdEstadoInmuebleNavigation.Descripcion))
                .FirstOrDefaultAsync();
        }

        private sealed record DatosInmueble(string Codigo, string Titulo, string Estado);

        /// <summary>
        /// Ejecuta un procedimiento y traduce el error de SQL Server a un
        /// mensaje de negocio. Los THROW de los procedimientos y de los
        /// triggers usan numeros mayores a 50000, asi que se pueden separar
        /// del ruido tecnico y mostrarse tal cual al usuario.
        /// </summary>
        private static async Task EjecutarAsync(Func<Task<int>> procedimiento)
        {
            try
            {
                await procedimiento();
            }
            catch (SqlException ex)
            {
                throw new BusinesException(MensajeDeSql(ex));
            }
        }

        private static string MensajeDeSql(SqlException ex)
        {
            var propios = ex.Errors
                .Cast<SqlError>()
                .Where(e => e.Number >= 50000)
                .Select(e => e.Message)
                .ToList();

            return propios.Count > 0 ? string.Join(" ", propios) : ex.Message;
        }
    }
}
