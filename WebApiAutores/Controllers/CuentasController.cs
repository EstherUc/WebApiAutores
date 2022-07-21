using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAutores.DTOs;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/cuentas")]
    public class CuentasController: ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;

        public CuentasController(UserManager<IdentityUser> userManager,IConfiguration configuration)
        {
            this.userManager = userManager;
            this.configuration = configuration;
        }

        [HttpPost("registrar")] //api/cuentas/resgistrar
        public async Task<ActionResult<RespuestaAutenticacion>> Registrar(CredencialesUsusario credencialesUsusario)
        {
            var usuario = new IdentityUser { UserName = credencialesUsusario.Email, Email = credencialesUsusario.Email };
            var resultado = await userManager.CreateAsync(usuario, credencialesUsusario.Password);

            if (resultado.Succeeded)
            {
                //retornamos JWT (Json Web Token)
                return ConstruirToken(credencialesUsusario);

            }
            else
            {
                return BadRequest(resultado.Errors);
            }
        }

        private RespuestaAutenticacion ConstruirToken(CredencialesUsusario credencialesUsusario)
        {
            //listado de Claims. Claim es una información sobre el usuario en la cual podemos confiar
            //En los Claims no se pone información sensible porque el cliente de la app tmb va a poder ver los claims. No son secrets.
            //Aquí no se pone ni pwd, ni num tarjetas, ni ese tipo de cosas, NUNCA tienen que viajar en un Claim esa información.
            var claims = new List<Claim>()
            {
                //par clave, valor
                new Claim("email", credencialesUsusario.Email),
            };

            //Contruir JWT
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);

            //contruir token
            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, 
                expires: expiracion, signingCredentials: creds);

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };
        }
    }
}
