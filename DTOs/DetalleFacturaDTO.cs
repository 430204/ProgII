
namespace InmoWeb.DTOs
{
    public class DetalleFacturaDTO
    {
        public int IdFactura { get; set; }

        public string Concepto { get; set; } = "";

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
    }
}

