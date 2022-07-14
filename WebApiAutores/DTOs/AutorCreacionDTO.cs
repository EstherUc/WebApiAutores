using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs
{
    public class AutorCreacionDTO
    {
        //Aquí se pone todo lo que necesito para crear un autor
        [Required(ErrorMessage = "El campo {0} es obligarotio")]
        [StringLength(maximumLength: 30, ErrorMessage = "El campo {0} no debe tener más de {1} carácteres")]
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }
    }
}
