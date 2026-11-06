using InmoWeb.DTOs;

namespace InmoWeb.Repositories.Persona.Interfaz
{
    public interface IPersonaRepository
    {
        Task<bool> login(UserLoginDTO user);
    }
}
