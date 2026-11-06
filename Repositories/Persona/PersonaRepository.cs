using InmoWeb.DTOs;
using InmoWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace InmoWeb.Repositories.Persona
{
    public class PersonaRepository
    {

        private readonly InmobiliariaDBContext db;

        public PersonaRepository(InmobiliariaDBContext db)
        {
            this.db = db;
        }


        public async Task<bool> login(UserLoginDTO user)
        {
            return await db.Personas.FirstOrDefaultAsync(x => x.Apellido.Equals(user.Apellido) && x.Dni.Equals(user.DNI)) != null;
        }
    }
}
