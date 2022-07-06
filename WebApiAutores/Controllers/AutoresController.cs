using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;
using WebApiAutores.Servicios;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/autores")]
    //[Authorize]
    public class AutoresController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IServicio servicio;
        private readonly ServicioTransient servicioTransient;
        private readonly ServicioSingleton servicioSingleton;
        private readonly ServicioScoped servicioScoped;
        private readonly ILogger<AutoresController> logger;

        public AutoresController(ApplicationDbContext context, IServicio servicio, ServicioTransient servicioTransient,
            ServicioSingleton servicioSingleton, ServicioScoped servicioScoped, ILogger<AutoresController> logger)
        {
            this.context = context;
            this.servicio = servicio;
            this.servicioTransient = servicioTransient;
            this.servicioSingleton = servicioSingleton;
            this.servicioScoped = servicioScoped;
            this.logger = logger;
        }

        [HttpGet("GUID")]   
        //[ResponseCache(Duration = 10)] //FILTRO.Guarda en cache durante 10 segundos la respuesta de la acción, Durante esos 10 segundos si hay petición le devuelve la respuesta guardada en cache
        [ServiceFilter(typeof(MiFiltroDeAccion))]
        public ActionResult ObtenerGuids()
        {
            return Ok( new { 
                    AutoresController_Transient = servicioTransient.Guid,
                    ServicioA_Transient = servicio.ObtenerTransient(),
                    AutoresController_Scoped = servicioScoped.Guid,
                    ServicioA_Scoped = servicio.ObtenerScoped(),
                    AutoresController_Singleton = servicioSingleton.Guid,                 
                    ServicioA_Singleton = servicio.ObtenerSingleton()
            });
        }

        [HttpGet] //api/autores
        [HttpGet("listado")] //api/autores/listado
        [HttpGet("/listado")] //listado
        //[ResponseCache(Duration = 10)]
        [ServiceFilter(typeof(MiFiltroDeAccion))]
        public async Task<ActionResult<List<Autor>>> Get()
        {
            throw new NotImplementedException();
            logger.LogInformation("Estamos obteniendo los autores");
           // servicio.RealizarTarea();
            return await context.Autores.Include(x => x.Libros).ToListAsync();
        }

        [HttpGet("primero")] //api/autores/primero?nombre=esther  (esther es un ejemplo, sería lo que escribieran, pero así quedaría la url)
        public async Task<ActionResult<Autor>> PrimerAutor([FromHeader] int miValor, [FromQuery] string nombre)
        {
            return await context.Autores.FirstOrDefaultAsync();
        }

        [HttpGet("{id:int}/{param2=persona}")] //param2 por defecto persona en este ejemplo. Si quisieramos el parametro opcional se pondría param2? y param2 sin asignar nada ni interrogación para que pongan (manden un parametro) lo que quieran pero si no ponen nada daría error
        public async Task<ActionResult<Autor>> Get(int id, string param2)
        {
            var autor =  await context.Autores.FirstOrDefaultAsync(x => x.Id == id);   

            if (autor == null)
            {
                return NotFound();
            }

            return autor;  
        }

        [HttpGet("{nombre}")]
        public async Task<ActionResult<Autor>> Get([FromRoute] string nombre) //[FromRoute] significa que el dato va a venir desde la ruta
        {
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Nombre.Contains(nombre));

            if (autor == null)
            {
                return NotFound();
            }

            return autor;
        }


        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Autor autor) //[FromBody] significa que el dato va a venir del cuerpo de la petición http
        {
            var existeAutorConElMismoNombre = await context.Autores.AnyAsync(x => x.Nombre == autor.Nombre);

            if (existeAutorConElMismoNombre)
            {
                return BadRequest($"Ya existe un autor con el nombre {autor.Nombre}"); //Es un Error 400
            }

            context.Add(autor);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(Autor autor, int id)
        {
            if (autor.Id != id)
            {
                return BadRequest("El id del autor no coincide con el id de la URL");
            }

            var existe = await context.Autores.AnyAsync(autor => autor.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Update(autor);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Autores.AnyAsync(autor => autor.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Autor { Id = id });
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
