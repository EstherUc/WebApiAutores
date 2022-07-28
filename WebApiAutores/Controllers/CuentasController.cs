using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAutores.DTOs;
using WebApiAutores.Servicios;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/cuentas")]
    public class CuentasController: ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly HashService hashService;
        private readonly IDataProtector dataProtector;

        public CuentasController(UserManager<IdentityUser> userManager,IConfiguration configuration,
            SignInManager<IdentityUser> signInManager, IDataProtectionProvider dataProtectionProvider,
            HashService hashService)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.hashService = hashService;
            dataProtector = dataProtectionProvider.CreateProtector("valor_unico_y_quizas_secreto");//se pondria un string con valor aleatorio o algo por el estilo
        }

        /*
        [HttpGet("hash/{textoPlano}")]
        public ActionResult RealizarHash(string textoPlano)
        {
            //llega el textoPlano del cliente, le hago dos veces hash, para ver que al hacer dos hash construye dos sal aleatoria que van a ser distintas y por consiguiente el resultado de los hash tb seran distintos
            var resultado1 = hashService.Hash(textoPlano);
            var resultado2 = hashService.Hash(textoPlano);

            return Ok(new
            {
                textoPlano = textoPlano,
                Hash1 = resultado1,
                Hash2 = resultado2
            });

        }


        //prueba encriptación
        [HttpGet("encriptar")]
        public ActionResult Encriptar()
        {
            var textoPlano = "Esther Uclés";
            var textoCifrado = dataProtector.Protect(textoPlano);
            var textoDesencriptado = dataProtector.Unprotect(textoCifrado);

            return Ok(new
            {
                textoPlano = textoPlano,
                textoCifrado = textoCifrado,
                textoDesencriptado = textoDesencriptado
            });
        }

        //prueba encriptación por tiempo
        [HttpGet("encriptarPorTiempo")]
        public ActionResult EncriptarPorTiempo()
        {
            var protectorLimitadoPorTiempo = dataProtector.ToTimeLimitedDataProtector();

            var textoPlano = "Esther Uclés";
            var textoCifrado = protectorLimitadoPorTiempo.Protect(textoPlano, lifetime: TimeSpan.FromSeconds(5));
            //Thread.Sleep(6000);//con esto se paraliza 6 segundos el hilo que se esta ejecutando (para probar el tiempo de encriptado y vemos que da una excepcion de expired)
            var textoDesencriptado = protectorLimitadoPorTiempo.Unprotect(textoCifrado);

            return Ok(new
            {
                textoPlano = textoPlano,
                textoCifrado = textoCifrado,
                textoDesencriptado = textoDesencriptado
            });
        }
        */

        [HttpPost("registrar", Name = "registrarUsuario")] //api/cuentas/resgistrar
        public async Task<ActionResult<RespuestaAutenticacion>> Registrar(CredencialesUsusario credencialesUsusario)
        {
            var usuario = new IdentityUser { UserName = credencialesUsusario.Email, Email = credencialesUsusario.Email };
            var resultado = await userManager.CreateAsync(usuario, credencialesUsusario.Password);

            if (resultado.Succeeded)
            {
                //retornamos JWT (Json Web Token)
                return await ConstruirToken(credencialesUsusario);

            }
           
            return BadRequest(resultado.Errors);
            
        }

        [HttpPost("login", Name = "loginUsuario")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login(CredencialesUsusario credencialesUsusario)
        {
            var resultado = await  signInManager.PasswordSignInAsync(credencialesUsusario.Email, credencialesUsusario.Password,
                isPersistent: false, lockoutOnFailure: false); //isPersistent es para la cookie de identificacion (en este caso no usamos)
                                                               //y lockoutOnFailure que es que el usuario debe de ser bloqueado si los intentos del logeo no son satisfactorios

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsusario);
            }
            
            return BadRequest("Login incorrecto");
            
        }

        //Este HttpGet es para renovar el token (construyendo un nuevo token) en segundo plano (con un nuevo tiempo de expiración)
        //para que cuando el usuario esta en activo y pasa el tiempo y expira su token no se quede desactivado durante su actividad
        [HttpGet("RenovarToken", Name = "renovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAutenticacion>> Renovar()
        {
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;
            var credencialesUsuario = new CredencialesUsusario()
            {
                Email = email
            };

            return await ConstruirToken(credencialesUsuario);
        }

        private async Task<RespuestaAutenticacion> ConstruirToken(CredencialesUsusario credencialesUsusario)
        {
            //listado de Claims. Claim es una información sobre el usuario en la cual podemos confiar
            //En los Claims no se pone información sensible porque el cliente de la app tmb va a poder ver los claims. No son secrets.
            //Aquí no se pone ni pwd, ni num tarjetas, ni ese tipo de cosas, NUNCA tienen que viajar en un Claim esa información.
            var claims = new List<Claim>()
            {
                //par clave, valor
                new Claim("email", credencialesUsusario.Email),
            };

            var usuario = await userManager.FindByEmailAsync(credencialesUsusario.Email);
            var claimsBD = await userManager.GetClaimsAsync(usuario);

            claims.AddRange(claimsBD);


            //Contruir JWT
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1); //pongo un año para no estar renovando tokens con frecuencia. Para pruebas

            //contruir token
            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, 
                expires: expiracion, signingCredentials: creds);

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };
        }

        [HttpPost("HacerAdmin", Name = "hacerAdmin")]
        public async Task<ActionResult> HacerAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            await userManager.AddClaimAsync(usuario, new Claim("esAdmin", "1"));
            return NoContent();
        }

        [HttpPost("EliminarAdmin", Name = "eliminarAdmin")]
        public async Task<ActionResult> EliminarAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            await userManager.RemoveClaimAsync(usuario, new Claim("esAdmin", "1"));
            return NoContent();
        }
    }
}
