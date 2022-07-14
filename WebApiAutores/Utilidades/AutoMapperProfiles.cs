using AutoMapper;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Utilidades
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AutorCreacionDTO, Autor>(); //desde donde (en este caso AutorCreacionDTO) y hacia donde mapeamos (en este caso hacia Autor)
            CreateMap<Autor, AutorDTO>();
            CreateMap<LibroCreacionDTO, Libro>()
                .ForMember(libro => libro.AutoresLibros, opciones => opciones.MapFrom(MapAutoresLibros));
            CreateMap<Libro, LibroDTO>();
            CreateMap<ComentarioCreacionDTO, Comentario>();
            CreateMap<Comentario, ComentarioDTO>();
        }

        private List<AutoresLibro> MapAutoresLibros(LibroCreacionDTO libroCreacionDTO, Libro libro)
        {
            var resultado = new List<AutoresLibro>();
            
            if(libroCreacionDTO.AutoresIds == null) //si no hay autores
            {
                return resultado;
            }

            //Si si hay autores:
            foreach (var autorId in libroCreacionDTO.AutoresIds)
            {
                resultado.Add(new AutoresLibro() { AutorId = autorId });
            }

            return resultado;
        }
    }
}
