
using InmoWeb.DTOs;
using InmoWeb.Repositories.DetalleFactura.Interfaz;
using InmoWeb.Services.DetalleFactura.Interfaces;

namespace InmoWeb.Services.DetalleFactura
{
    public class DetalleFacturaServices : IDetalleFacturaServices
    {
        private readonly IDetalleFacturaRepository repository;

        public DetalleFacturaServices(IDetalleFacturaRepository repository)
        {
            this.repository = repository;
        }

        public async Task<List<DetalleFacturaRespuestaDTO>> GetDetalles()
        {
            var detalles = await repository.GetDetalles();

            return detalles.Select(d => new DetalleFacturaRespuestaDTO
            {
                IdDetalleFactura = d.IdDetalleFactura,
                IdFactura = d.IdFactura,
                Concepto = d.Concepto,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal
            }).ToList();
        }

        public async Task<DetalleFacturaRespuestaDTO?> GetById(int id)
        {
            var detalle = await repository.GetById(id);

            if (detalle == null)
                return null;

            return new DetalleFacturaRespuestaDTO
            {
                IdDetalleFactura = detalle.IdDetalleFactura,
                IdFactura = detalle.IdFactura,
                Concepto = detalle.Concepto,
                Cantidad = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario,
                Subtotal = detalle.Subtotal
            };
        }

        public async Task<DetalleFacturaRespuestaDTO> Agregar(DetalleFacturaDTO dto)
        {
            ValidarDatos(dto);

            if (!await repository.ExisteFactura(dto.IdFactura))
                throw new ArgumentException("La factura no existe.");

            var detalle = new Models.DetalleFactura
            {
                IdFactura = dto.IdFactura,
                Concepto = dto.Concepto.Trim(),
                Cantidad = dto.Cantidad,
                PrecioUnitario = dto.PrecioUnitario,
                Subtotal = dto.Cantidad * dto.PrecioUnitario
            };

            await repository.Agregar(detalle);
            await repository.GuardarCambios();

            return new DetalleFacturaRespuestaDTO
            {
                IdDetalleFactura = detalle.IdDetalleFactura,
                IdFactura = detalle.IdFactura,
                Concepto = detalle.Concepto,
                Cantidad = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario,
                Subtotal = detalle.Subtotal
            };
        }

        public async Task<bool> Modificar(int id, DetalleFacturaDTO dto)
        {
            ValidarDatos(dto);

            var detalle = await repository.GetById(id);

            if (detalle == null)
                return false;

            if (!await repository.ExisteFactura(dto.IdFactura))
                throw new ArgumentException("La factura no existe.");

            detalle.IdFactura = dto.IdFactura;
            detalle.Concepto = dto.Concepto.Trim();
            detalle.Cantidad = dto.Cantidad;
            detalle.PrecioUnitario = dto.PrecioUnitario;
            detalle.Subtotal = dto.Cantidad * dto.PrecioUnitario;

            repository.Modificar(detalle);
            await repository.GuardarCambios();

            return true;
        }

        public async Task<bool> Eliminar(int id)
        {
            var detalle = await repository.GetById(id);

            if (detalle == null)
                return false;

            repository.Eliminar(detalle);
            await repository.GuardarCambios();

            return true;
        }

        private void ValidarDatos(DetalleFacturaDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Concepto))
                throw new ArgumentException("El concepto es obligatorio.");

            if (dto.Concepto.Trim().Length > 200)
                throw new ArgumentException("El concepto no puede superar los 200 caracteres.");

            if (dto.Cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero.");

            if (dto.PrecioUnitario < 0)
                throw new ArgumentException("El precio no puede ser negativo.");

            if (decimal.Round(dto.PrecioUnitario, 2) != dto.PrecioUnitario)
                throw new ArgumentException("El precio admite hasta dos decimales.");

            if (dto.Cantidad * dto.PrecioUnitario > 999999999999.99m)
                throw new ArgumentException("El subtotal supera el máximo permitido.");
        }
    }
}

