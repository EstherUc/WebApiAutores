using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Entidades
{
    public class Autor: IValidatableObject //validación a nivel de modelo
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligarotio")]
        [StringLength(maximumLength:30, ErrorMessage ="El campo {0} no debe tener más de {1} carácteres")]
        //[PrimeraLetraMayuscula]
        public string Nombre { get; set; }
        public List<Libro> Libros { get; set; } //Propiedad de navegación

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) //validación a nivel de modelo
        {
            if (!string.IsNullOrEmpty(Nombre))
            {
                var primeraLetra = Nombre[0].ToString(); //cogemos la primera letra del nombre

                if(primeraLetra != primeraLetra.ToUpper())
                {
                    yield return new ValidationResult("La primera letra debe ser mayúscula", new string[] { nameof(Nombre) });
                }
            }

        }
    }
}
