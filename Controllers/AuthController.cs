using InmoWeb.DTOs;
using InmoWeb.Repositories.Persona.Interfaz;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InmoWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // Misma clave, emisor y audiencia que valida Program.cs
        private const string ClaveSecreta = "clave-super-secreta-para-poc-123456";
        private const string Emisor = "JwtAuthPoc";
        private const string Audiencia = "JwtAuthPocUser";
        private const int MinutosDeVigencia = 30;

        private readonly IPersonaRepository _repository;

        public AuthController(IPersonaRepository userRepository)
        {
            _repository = userRepository;
        }

        [HttpPost]
        public async Task<IActionResult> login([FromBody] UserLoginDTO user)
        {
            try
            {
                if (user == null)
                    return BadRequest(new { mensaje = "Se requiere objeto LOGIN" });

                if (!await _repository.login(user))
                    return Unauthorized(new { mensaje = "Apellido y/o DNI incorrecto" });

                var claims = new Claim[]
                {
                    new Claim("usuario", user.Apellido)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ClaveSecreta));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var expiracion = DateTime.UtcNow.AddMinutes(MinutosDeVigencia);

                var token = new JwtSecurityToken(
                    issuer: Emisor,
                    audience: Audiencia,
                    claims: claims,
                    expires: expiracion,
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new
                {
                    token = tokenString,
                    usuario = user.Apellido,
                    expira = expiracion
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { mensaje = $"Error interno : {ex.Message}" });
            }
        }
    }
}
