
using InmoWeb.DTOs;
using InmoWeb.Models;
using InmoWeb.Repositories.Factura.Interfaz;
using Microsoft.EntityFrameworkCore;

namespace InmoWeb.Repositories.Factura
{
    public class FacturaRepository : IFacturaRepository
    {
        private readonly InmobiliariaDBContext db;

        public FacturaRepository(InmobiliariaDBContext db)
        {
            this.db = db;
        }

        public async Task<List<FacturaDTO>> GetFacturas()
        {
            return await db.Facturas
                .AsNoTracking()
                .OrderBy(f => f.IdFactura)
                .Select(f => new FacturaDTO
                {
                    IdFactura = f.IdFactura,
                    NumeroFactura = f.NumeroFactura,
                    Estado = f.Estado
                })
                .ToListAsync();
        }
    }
}
