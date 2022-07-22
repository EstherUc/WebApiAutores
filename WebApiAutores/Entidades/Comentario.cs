using Microsoft.AspNetCore.Identity;

namespace WebApiAutores.Entidades
{
    public class Comentario
    {
        public int Id { get; set; }
        public string Contenido { get; set; }
        public int LibroId { get; set; }
        public Libro Libro { get; set; } //propiedad de navegación
        public string UsuarioId { get; set; }
        public IdentityUser Usuario { get; set; }
    }
}
