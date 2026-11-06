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
        
            private IPersonaRepository _repository;

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
                        return BadRequest(new { message = "Se requiere objeto LOGIN" });
                    if (! await _repository.login(user))
                        return Unauthorized(new { message = "Usuario y/o Password incorrecto" });

                    var claims = new Claim[]
                    {
                    new Claim("usuario", user.Apellido)
                    };

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("clave-super-secreta-para-poc-123456"));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                        issuer: "JwtAuthPoc",
                        audience: "JwtAuthPocUser",
                        claims: claims,
                        expires: DateTime.UtcNow.AddMinutes(30),
                        signingCredentials: creds
                    );

                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                    return Ok(tokenString);
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = $"Error interno : {ex}" });
                }

            }
    }
}
