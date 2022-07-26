﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
        private readonly SignInManager<IdentityUser> signInManager;

        public CuentasController(UserManager<IdentityUser> userManager,IConfiguration configuration,
            SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
        }

        [HttpPost("registrar")] //api/cuentas/resgistrar
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

        [HttpPost("login")]
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
        [HttpGet("RenovarToken")]
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

        [HttpPost("HacerAdmin")]
        public async Task<ActionResult> HacerAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            await userManager.AddClaimAsync(usuario, new Claim("esAdmin", "1"));
            return NoContent();
        }

        [HttpPost("EliminarAdmin")]
        public async Task<ActionResult> EliminarAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            await userManager.RemoveClaimAsync(usuario, new Claim("esAdmin", "1"));
            return NoContent();
        }
    }
}
