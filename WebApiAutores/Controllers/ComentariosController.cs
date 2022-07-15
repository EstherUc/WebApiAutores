using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/libros/{libroId:int}/comentarios")] //cada comentario depende del libro
    public class ComentariosController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public ComentariosController(ApplicationDbContext context,
            IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId)
        {
            //comprobar si libroId existe
            var exiteLibro = await context.Libros.AnyAsync(librosBD
                => librosBD.Id == libroId);

            if (!exiteLibro)
            {
                return NotFound(); //devolvemos un 404
            }

            var comentarios = await context.Comentarios.Where(comentarioBD => comentarioBD.LibroId == libroId).ToListAsync();

            return mapper.Map<List<ComentarioDTO>>(comentarios);    
        }

        [HttpGet("{id:int}", Name = "ObtenerComentario")]
        public async Task<ActionResult<ComentarioDTO>> GetPorId(int id)
        {
            var comentario = await context.Comentarios.FirstOrDefaultAsync(comentarioBD => comentarioBD.Id == id);  

            if(comentario == null)
            {
                return NotFound(); //retorna un 404
            }

            return mapper.Map<ComentarioDTO>(comentario);

        }

        [HttpPost]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            //comprobar si libroId existe
            var exiteLibro = await context.Libros.AnyAsync(librosBD 
                => librosBD.Id == libroId);

            if (!exiteLibro)
            {
                return NotFound(); //devolvemos un 404
            }

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.LibroId = libroId;
            context.Add(comentario);
            await context.SaveChangesAsync();

            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);

            return CreatedAtRoute("ObtenerComentario", new { id = comentario.Id, libroId = libroId }, comentarioDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int libroId, int id, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            //comprobar si libroId existe
            var exiteLibro = await context.Libros.AnyAsync(librosBD
                => librosBD.Id == libroId);

            if (!exiteLibro)
            {
                return NotFound(); //devolvemos un 404
            }

            var existeComentario = await context.Comentarios.AnyAsync(comentarioBD => comentarioBD.Id == id);

            if (!existeComentario)
            {
                return NotFound();
            }

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.Id = id;
            comentario.LibroId = libroId;
            context.Update(comentario);
            await context.SaveChangesAsync();
            return NoContent();
        }   
    }
}
