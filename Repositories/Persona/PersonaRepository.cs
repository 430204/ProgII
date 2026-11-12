using InmoWeb.DTOs;
using InmoWeb.Models;
using InmoWeb.Repositories.Persona.Interfaz;
using Microsoft.EntityFrameworkCore;

namespace InmoWeb.Repositories.Persona
{
    public class PersonaRepository : IPersonaRepository
    {

        private readonly InmobiliariaDBContext db;

        public PersonaRepository(InmobiliariaDBContext db)
        {
            this.db = db;
        }


        public async Task<bool> login(UserLoginDTO user)
        {
            if (user is null
                || string.IsNullOrWhiteSpace(user.Apellido)
                || string.IsNullOrWhiteSpace(user.DNI))
            {
                return false;
            }

            var apellido = user.Apellido.Trim();
            var dni = user.DNI.Trim();

            return await db.Personas
                .AsNoTracking()
                .AnyAsync(p => p.Apellido == apellido && p.Dni == dni);
        }
    }
}
