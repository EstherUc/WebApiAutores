﻿using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;
using WebApiAutores.Utilidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/autores")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class AutoresController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAuthorizationService authorizationService;

        public AutoresController(ApplicationDbContext context, IMapper mapper, IAuthorizationService authorizationService)
        {
            this.context = context;
            this.mapper = mapper;
            this.authorizationService = authorizationService;
        }

        /*[HttpGet("Configuraciones")]
        public ActionResult<string> ObtenerConfiguracion()
        { 
            return configuration["prueba"];

        }*/
        
        [HttpGet(Name = "obtenerAutores")] //api/autores
        [AllowAnonymous] //es una excepción del Authorize (lo tenemos puesto a nivel de controlador) y Permite que no autentificados puedan consumir este EndPoint
        public async Task<IActionResult> Get([FromQuery] bool incluirHATEOAS = true)
        {
            var autores = await context.Autores.ToListAsync();
            var dtos = mapper.Map<List<AutorDTO>>(autores);
            

            if (incluirHATEOAS)
            {
                var esAdmin = await authorizationService.AuthorizeAsync(User, "esAdmin");

                //dtos.ForEach(dto => GenerarEnlaces(dto, esAdmin.Succeeded));//Por cada autor de la lista generamos un enlace para dicho autor

                var resultado = new ColeccionDeRecursos<AutorDTO> { Valores = dtos };
                resultado.Enlaces.Add(new DatoHATEOAS(
                    enlace: Url.Link("obtenerAutores", new { }),
                    descripcion: "self",
                    metodo: "GET"));

                if (esAdmin.Succeeded)
                {
                    resultado.Enlaces.Add(new DatoHATEOAS(
                   enlace: Url.Link("crearAutor", new { }),
                   descripcion: "crear-autor",
                   metodo: "POST"));
                }

                return Ok(resultado);

            }

            return Ok(dtos);
        }
        

        [HttpGet("{id:int}", Name = "obtenerAutorPorId")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<AutorDTOConLibros>> Get(int id, [FromHeader] string incluirHATEOAS)
        {
            var autor =  await context.Autores
                .Include(autorBD => autorBD.AutoresLibros)
                .ThenInclude(autorLibroBD => autorLibroBD.Libro)
                .FirstOrDefaultAsync(autorBD => autorBD.Id == id);   

            if (autor == null)
            {
                return NotFound(); //devuelve un 404
            }

            var dto = mapper.Map<AutorDTOConLibros>(autor);

            return dto;
        }

       

        [HttpGet("{nombre}", Name = "obtenerAutorPorNombre")]
        public async Task<ActionResult<List<AutorDTO>>> Get([FromRoute] string nombre) //[FromRoute] significa que el dato va a venir desde la ruta
        {
            var autores = await context.Autores.Where(autorBD => autorBD.Nombre.Contains(nombre)).ToListAsync();

            return mapper.Map<List<AutorDTO>>(autores);
        }


        [HttpPost(Name = "crearAutor")]
        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO autorCreacionDTO) //[FromBody] significa que el dato va a venir del cuerpo de la petición http
        {
            var existeAutorConElMismoNombre = await context.Autores.AnyAsync(x => x.Nombre == autorCreacionDTO.Nombre);

            if (existeAutorConElMismoNombre)
            {
                return BadRequest($"Ya existe un autor con el nombre {autorCreacionDTO.Nombre}"); //Es un Error 400
            }
            //hacemos el mapeo con AutoMapper
            var autor = mapper.Map<Autor>(autorCreacionDTO);

            context.Add(autor);
            await context.SaveChangesAsync();

            var autorDTO = mapper.Map<AutorDTO>(autor);

            return CreatedAtRoute("obtenerAutor", new { id = autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}", Name = "actualizarAutor")]
        public async Task<ActionResult> Put(AutorCreacionDTO autorCreacionDTO, int id)
        {

            var existe = await context.Autores.AnyAsync(autor => autor.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;

            context.Update(autor);
            await context.SaveChangesAsync();
            return NoContent(); //retorna un 204
        }

        [HttpDelete("{id:int}", Name = "borrarAutor")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(autor => autor.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Autor() { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
