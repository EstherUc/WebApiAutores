using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/libros")]
    public class LibrosController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public LibrosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        
        [HttpGet("{id:int}", Name = "ObtenerLibro")]   
        public async Task<ActionResult<LibroDTOConAutores>> Get(int id)
        {
            var libro = await context.Libros
                .Include(libroBD => libroBD.AutoresLibros)
                .ThenInclude(autorLibroBD => autorLibroBD.Autor)
                .FirstOrDefaultAsync(x => x.Id == id); // para incluir comentario: .Include(libroBD => libroBD.Comentarios)
            
            if(libro == null)
            {
                return NotFound();
            }

            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();

            return mapper.Map<LibroDTOConAutores>(libro);
        }

        [HttpPost]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO.AutoresIds == null)
            {
                return BadRequest("No se puede crear un libro sin autores");
            }

            var autoresIds = await context.Autores.Where(autorBD => libroCreacionDTO.AutoresIds.Contains(autorBD.Id))
                 .Select(x => x.Id).ToListAsync();

            if(libroCreacionDTO.AutoresIds.Count != autoresIds.Count)
            {
                return BadRequest("No existe uno de los autores enviados");
            }

            var libro = mapper.Map<Libro>(libroCreacionDTO);
            AsignarOrdenAutores(libro);

            context.Add(libro);
            await context.SaveChangesAsync();
            
            var libroDTO = mapper.Map<LibroDTO>(libro);

            return CreatedAtRoute("ObtenerLibro", new { id = libro.Id }, libroDTO);
        } 

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put (int id, LibroCreacionDTO libroCreacionDTO)
        {
            //en libroBD estoy trayendo el libro cuyo id me han pasado y estoy incluyendo el listado de autores, libros... para poder actualizarlo
            var libroBD = await context.Libros
                .Include(x => x.AutoresLibros)
                .FirstOrDefaultAsync(x => x.Id == id);

            if(libroBD == null)
            {
                return NotFound(); 
            }

            libroBD = mapper.Map(libroCreacionDTO, libroBD);//con el automapper paso las propiedades de libroCreacionDTO hacia libroBD guardandolo en la instancia libroBD, esto me permite hacer una actualizacion de la entidad Libros y AutoresLibros
            AsignarOrdenAutores(libroBD);

            await context.SaveChangesAsync();
            return NoContent();
        }

        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.AutoresLibros != null)
            {
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }

        }

        [HttpPatch("{id:int}")]

        public async Task<ActionResult> Patch(int id, JsonPatchDocument<LibroPatchDTO> patchDocument)
        {
            if (patchDocument == null) // si esto es null es porque hay un error con el formato que nos ha enviado el cliente
            {
                return BadRequest();
            }


            var libroBD = await context.Libros.FirstOrDefaultAsync(x => x.Id == id);

            if (libroBD == null) //si el libro no existe en la BD
            {
                return NotFound();
            }

            var libroDTO = mapper.Map<LibroPatchDTO>(libroBD);//aqui se esta rellenando LibroPatchDTO con la informacion del libroBD

            patchDocument.ApplyTo(libroDTO, ModelState); //aquí estamos aplicando a LibroDTO los cambios que vienen en el patchDocument

            var esValido = TryValidateModel(libroDTO);

            if (!esValido)
            {
                return BadRequest(ModelState);
            }

            mapper.Map(libroDTO,libroBD);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Libros.AnyAsync(libro => libro.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Libro() { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}


