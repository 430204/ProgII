
namespace InmoWeb.DTOs
{
    public class DetalleFacturaRespuestaDTO
    {
        public int IdDetalleFactura { get; set; }
        public int IdFactura { get; set; }
        public string Concepto { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}

