
using InmoWeb.Models;
using InmoWeb.Repositories.DetalleFactura.Interfaz;
using Microsoft.EntityFrameworkCore;

namespace InmoWeb.Repositories.DetalleFactura
{
    public class DetalleFacturaRepository : IDetalleFacturaRepository
    {
        private readonly InmobiliariaDBContext db;

        public DetalleFacturaRepository(InmobiliariaDBContext db)
        {
            this.db = db;
        }

        public async Task<List<Models.DetalleFactura>> GetDetalles()
        {
            return await db.DetalleFacturas
                .AsNoTracking()
                .OrderBy(d => d.IdDetalleFactura)
                .ToListAsync();
        }

        public async Task<Models.DetalleFactura?> GetById(int id)
        {
            return await db.DetalleFacturas
                .FindAsync(id);
        }

        public async Task<bool> ExisteFactura(int idFactura)
        {
            return await db.Facturas
                .AnyAsync(f => f.IdFactura == idFactura);
        }

        public async Task Agregar(Models.DetalleFactura detalle)
        {
            await db.DetalleFacturas.AddAsync(detalle);
        }

        public void Modificar(Models.DetalleFactura detalle)
        {
            db.DetalleFacturas.Update(detalle);
        }

        public void Eliminar(Models.DetalleFactura detalle)
        {
            db.DetalleFacturas.Remove(detalle);
        }

        public async Task GuardarCambios()
        {
            await db.SaveChangesAsync();
        }
    }
}
